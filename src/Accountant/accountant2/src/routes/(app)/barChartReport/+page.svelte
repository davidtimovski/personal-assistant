<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { resolve } from '$app/paths';
	import { BarController, BarElement, CategoryScale, Chart, LinearScale } from 'chart.js';

	import { DateHelper } from '../../../../../../Core/shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { FromOption } from '$lib/models/viewmodels/fromOption';
	import { CategoryType } from '$lib/models/entities/category';
	import type { TransactionModel } from '$lib/models/entities/transaction';
	import { AmountByMonth } from '$lib/models/viewmodels/amountByMonth';

	let mainAccountId: number | null = $state(null);
	let currency: string | null = $state(null);
	let chart: Chart;
	let fromOptions: FromOption[] | null = $state(null);
	let categoryOptions: SelectOption[] | null = $state(null);
	let balanceAverage: number | null = $state(null);
	let balanceTotal: number | null = $state(null);
	let spentAverage: number | null = $state(null);
	let spentTotal: number | null = $state(null);
	let depositedAverage: number | null = $state(null);
	let depositedTotal: number | null = $state(null);
	let savedAverage: number | null = $state(null);
	let savedTotal: number | null = $state(null);
	let canvas: HTMLCanvasElement;
	let canvasCtx: CanvasRenderingContext2D | null = null;
	let categoryId = $state(0);
	let categoryType = $state(CategoryType.AllTransactions);
	let type = $state(TransactionType.Any);

	const localStorage = new LocalStorageUtil();
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;
	let categoriesService: CategoriesService;

	const now = new Date();
	const from = new Date(now.getFullYear(), now.getMonth() - 6, 1);
	let fromDate = $state(DateHelper.format(from));

	Chart.register(BarController, BarElement, CategoryScale, LinearScale);
	Chart.defaults.font.family = '"Didact Gothic", sans-serif';

	function groupBy(list: TransactionModel[], keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }) {
		const map = new Map();
		list.forEach((item) => {
			const key = keyGetter(item);
			const collection = map.get(key);
			if (!collection) {
				map.set(key, [item]);
			} else {
				collection.push(item);
			}
		});
		return map;
	}

	async function loadData() {
		if (mainAccountId === null || currency === null) {
			throw new Error('Data not initialized yet');
		}

		chart.data.labels = [];
		chart.data.datasets[0].data = [];
		chart.update();

		if (categoryId) {
			const category = await categoriesService.get(categoryId);
			categoryType = category.type;

			if (category.type === CategoryType.DepositOnly) {
				type = TransactionType.Deposit;
			} else if (category.type === CategoryType.ExpenseOnly) {
				type = TransactionType.Expense;
			}
		} else {
			categoryType = CategoryType.AllTransactions;
		}

		const transactions = await transactionsService.getForBarChart(fromDate, mainAccountId, categoryId, type, currency);

		const itemGroups = groupBy(transactions, (x: TransactionModel) => DateHelper.formatYYYYMM(new Date(x.date)));

		const from = new Date(fromDate);
		const now = new Date();

		const monthsDiff = now.getMonth() - from.getMonth() + 12 * (now.getFullYear() - from.getFullYear());

		let balanceSum = 0;
		let spentSum = 0;
		let depositedSum = 0;
		let savedSum = 0;

		const items = new Array<AmountByMonth>();
		for (let i = 0; i < monthsDiff; i++) {
			const date = DateHelper.formatYYYYMM(from);

			let monthString = DateHelper.getShortMonth(from, $user.language);
			if (from.getFullYear() < now.getFullYear()) {
				monthString += ' ' + from.getFullYear().toString().substring(2, 4);
			}

			if (itemGroups.has(date)) {
				let monthTransactions = new Array<TransactionModel>();
				for (const key of itemGroups.keys()) {
					if (key === date) {
						monthTransactions = itemGroups.get(key);
						break;
					}
				}

				const item = new AmountByMonth(date, monthString, 0);

				switch (type) {
					case TransactionType.Any:
						{
							for (const transaction of monthTransactions) {
								if (transaction.fromAccountId) {
									item.amount -= transaction.amount;
									spentSum += transaction.amount;
								} else {
									item.amount += transaction.amount;
									depositedSum += transaction.amount;
								}
							}
							balanceSum += item.amount;
						}
						break;
					case TransactionType.Expense:
						{
							item.amount -= monthTransactions.map((x) => x.amount).reduce((a, b) => a + b, 0);
							spentSum += item.amount;
						}
						break;
					case TransactionType.Deposit:
						{
							item.amount += monthTransactions.map((x) => x.amount).reduce((a, b) => a + b, 0);
							depositedSum += item.amount;
						}
						break;
					case TransactionType.Saving:
						{
							const movedFromMain = monthTransactions.filter((x) => x.fromAccountId === mainAccountId);
							const movedToMain = monthTransactions.filter((x) => x.toAccountId === mainAccountId);
							const depositedToInvestmentFunds = monthTransactions.filter(
								(x) => x.fromAccountId !== mainAccountId && x.toAccountId !== mainAccountId
							);

							item.amount += movedFromMain.map((x) => x.amount).reduce((a, b) => a + b, 0);
							item.amount -= movedToMain.map((x) => x.amount).reduce((a, b) => a + b, 0);
							item.amount += depositedToInvestmentFunds.map((x) => x.amount).reduce((a, b) => a + b, 0);

							savedSum += item.amount;
						}
						break;
				}

				item.amount = Formatter.truncateDecimals(item.amount, currency);

				items.push(item);
			} else {
				items.push(new AmountByMonth(date, monthString, 0));
			}

			from.setMonth(from.getMonth() + 1);
		}

		switch (type) {
			case TransactionType.Any:
				balanceAverage = balanceSum / monthsDiff;
				balanceTotal = balanceSum;
				spentAverage = Math.abs(spentSum) / monthsDiff;
				spentTotal = Math.abs(spentSum);
				depositedAverage = depositedSum / monthsDiff;
				depositedTotal = depositedSum;
				break;
			case TransactionType.Expense:
				spentAverage = Math.abs(spentSum) / monthsDiff;
				spentTotal = Math.abs(spentSum);
				break;
			case TransactionType.Deposit:
				depositedAverage = depositedSum / monthsDiff;
				depositedTotal = depositedSum;
				break;
			case TransactionType.Saving:
				savedAverage = savedSum / monthsDiff;
				savedTotal = savedSum;
				break;
		}

		const labels = new Array<string>();
		const amounts = new Array<number>();
		for (let i = 0; i < items.length; i++) {
			labels.push(items[i].month);
			amounts.push(items[i].amount);
		}

		if (labels.length > 0) {
			chart.data.labels = labels;

			const expenseColor = '#f55551';
			const depositColor = '#65c565';
			switch (type) {
				case TransactionType.Any:
				case TransactionType.Saving:
					{
						const colors = new Array<string>();
						for (const amount of amounts) {
							colors.push(amount < 0 ? expenseColor : depositColor);
						}

						chart.data.datasets[0].backgroundColor = colors;
					}
					break;
				case TransactionType.Expense:
					chart.data.datasets[0].backgroundColor = expenseColor;
					break;
				case TransactionType.Deposit:
					chart.data.datasets[0].backgroundColor = depositColor;
					break;
			}

			chart.data.datasets[0].data = amounts;
		} else {
			chart.data.labels = [];
			chart.data.datasets[0].data = [];
		}

		chart.update();
	}

	onMount(async () => {
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();
		categoriesService = new CategoriesService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const fromOpts = new Array<FromOption>();
		const monthsSince = [6, 12, 18, 24, 36, 48, 60, 72];
		for (let months of monthsSince) {
			const value = DateHelper.format(new Date(now.getFullYear(), now.getMonth() - months));

			const label = months + ' ' + $t('barChartReport.months');

			fromOpts.push(new FromOption(value, label));
		}
		fromOptions = fromOpts;

		const categoryOptionsArr = [new SelectOption(0, $t('barChartReport.all'))];
		categoriesService.getAllAsOptions($t('uncategorized'), CategoryType.AllTransactions).then((options) => {
			categoryOptions = categoryOptionsArr.concat(options);
		});

		canvasCtx = canvas.getContext('2d');
		chart = new Chart(<CanvasRenderingContext2D>canvasCtx, {
			type: 'bar',
			data: {
				datasets: [{ data: [] }]
			}
		});

		mainAccountId = await accountsService.getMainId();

		loadData();
	});

	onDestroy(() => {
		transactionsService?.release();
		accountsService?.release();
		categoriesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-chart-bar"></i>
		</div>
		<div class="page-title">{$t('barChartReport.barChart')}</div>
		<a href={resolve('/dashboard')} class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control inline">
				<label for="from-the-past">{$t('barChartReport.fromThePast')}</label>
				<select id="from-the-past" bind:value={fromDate} onchange={loadData} class="category-select">
					{#if fromOptions}
						{#each fromOptions as from (from.value)}
							<option value={from.value}>{from.label}</option>
						{/each}
					{/if}
				</select>
			</div>
			<div class="form-control inline">
				<label for="category">{$t('category')}</label>
				<div class="loadable-select" class:loaded={categoryOptions}>
					<select id="category" bind:value={categoryId} onchange={loadData} disabled={!categoryOptions} class="category-select">
						{#if categoryOptions}
							{#each categoryOptions as category (category.id)}
								<option value={category.id}>{category.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin"></i>
				</div>
			</div>
			<div class="form-control">
				<div class="multi-radio-wrap">
					{#if categoryType === CategoryType.AllTransactions}
						<div class="multi-radio-part">
							<label class:selected={type === TransactionType.Any}>
								<span>{$t('barChartReport.balance')}</span>
								<input type="radio" name="typeToggle" value={0} bind:group={type} onchange={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType !== CategoryType.DepositOnly}
						<div class="multi-radio-part">
							<label class:selected={type === TransactionType.Expense}>
								<span>{$t('barChartReport.expenses')}</span>
								<input type="radio" name="typeToggle" value={1} bind:group={type} onchange={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType !== CategoryType.ExpenseOnly}
						<div class="multi-radio-part">
							<label class:selected={type === TransactionType.Deposit}>
								<span>{$t('barChartReport.deposits')}</span>
								<input type="radio" name="typeToggle" value={2} bind:group={type} onchange={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType === CategoryType.AllTransactions}
						<div class="multi-radio-part">
							<label class:selected={type === TransactionType.Saving}>
								<span>{$t('barChartReport.savings')}</span>
								<input type="radio" name="typeToggle" value={4} bind:group={type} onchange={loadData} />
							</label>
						</div>
					{/if}
				</div>
			</div>
		</form>

		<div class="bar-chart-wrap">
			<canvas bind:this={canvas}></canvas>
		</div>

		<table class="summary-table">
			<thead>
				<tr>
					<th></th>
					<th>{$t('barChartReport.average')}</th>
					<th>{$t('barChartReport.total')}</th>
				</tr>
			</thead>
			<tbody>
				{#if type === TransactionType.Any}
					<tr>
						<td>{$t('balance')}</td>
						<td>{Formatter.money(balanceAverage, currency, $user.culture)}</td>
						<td>{Formatter.money(balanceTotal, currency, $user.culture)}</td>
					</tr>
				{/if}

				{#if type === TransactionType.Any || type === TransactionType.Expense}
					<tr>
						<td>{$t('barChartReport.spent')}</td>
						<td>{Formatter.money(spentAverage, currency, $user.culture)}</td>
						<td>{Formatter.money(spentTotal, currency, $user.culture)}</td>
					</tr>
				{/if}

				{#if type === TransactionType.Any || type === TransactionType.Deposit}
					<tr>
						<td>{$t('barChartReport.deposited')}</td>
						<td>{Formatter.money(depositedAverage, currency, $user.culture)}</td>
						<td>{Formatter.money(depositedTotal, currency, $user.culture)}</td>
					</tr>
				{/if}

				{#if type === TransactionType.Saving}
					<tr>
						<td>{$t('barChartReport.saved')}</td>
						<td>{Formatter.money(savedAverage, currency, $user.culture)}</td>
						<td>{Formatter.money(savedTotal, currency, $user.culture)}</td>
					</tr>
				{/if}
			</tbody>
		</table>
	</div>
</section>

<style lang="scss">
	.bar-chart-wrap {
		margin: 30px 0 50px;
	}

	.summary-table {
		width: 100%;

		th,
		td {
			padding: 6px 0 6px 15px;
			line-height: 1rem;
			text-align: right;
			white-space: nowrap;
		}

		th:first-child,
		td:first-child {
			padding: 6px 15px 6px 0;
			text-align: left;
		}

		th {
			border-bottom: 1px solid #ddd;
			line-height: 1.3rem;
			color: var(--primary-color);
		}
	}

	@media screen and (min-width: 1200px) {
		.summary-table {
			th,
			td {
				font-size: 1.1rem;
				line-height: 1.3rem;
			}
		}
	}
</style>
