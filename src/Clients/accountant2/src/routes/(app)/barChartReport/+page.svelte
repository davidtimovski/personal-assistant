<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { BarController, BarElement, CategoryScale, Chart, LinearScale } from 'chart.js';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { locale } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { CategoriesService } from '$lib/services/categoriesService';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';
	import { SelectOption } from '$lib/models/viewmodels/selectOption';
	import { FromOption } from '$lib/models/viewmodels/fromOption';
	import { CategoryType } from '$lib/models/entities/category';
	import type { TransactionModel } from '$lib/models/entities/transaction';
	import { AmountByMonth } from '$lib/models/viewmodels/amountByMonth';

	let mainAccountId: number;
	let currency: string;
	let chart: Chart;
	let fromOptions: FromOption[] | null = null;
	let categoryOptions: SelectOption[] | null = null;
	let balanceAverage: number;
	let spentAverage: number;
	let depositedAverage: number;
	let savedAverage: number;
	let canvas: HTMLCanvasElement;
	let canvasCtx: CanvasRenderingContext2D | null = null;
	let categoryId = 0;
	let categoryType = CategoryType.AllTransactions;
	let type = TransactionType.Any;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;
	let categoriesService: CategoriesService;

	const from = new Date();
	from.setMonth(from.getMonth() - 6);
	from.setDate(1);
	let fromDate = DateHelper.format(from);

	Chart.register(BarController, BarElement, CategoryScale, LinearScale);
	Chart.defaults.font.family = '"Didact Gothic", sans-serif';

	function groupBy(
		list: TransactionModel[],
		keyGetter: { (x: TransactionModel): string; (arg0: TransactionModel): any }
	) {
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

		let itemGroups = groupBy(
			transactions,
			(x: TransactionModel) => x.date.substring(0, 7) // yyyy-MM
		);

		const from = new Date(fromDate);
		const now = new Date();

		const monthsDiff = now.getMonth() - from.getMonth() + 12 * (now.getFullYear() - from.getFullYear());

		let balanceSum = 0;
		let spentSum = 0;
		let depositedSum = 0;
		let savedSum = 0;

		let items = new Array<AmountByMonth>();
		for (let i = 0; i < monthsDiff; i++) {
			const date = DateHelper.formatYYYYMM(from);

			let monthString = DateHelper.getShortMonth(from, $locale);
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

							item.amount += movedFromMain.map((x) => x.amount).reduce((a, b) => a + b, 0);
							item.amount -= movedToMain.map((x) => x.amount).reduce((a, b) => a + b, 0);

							savedSum += item.amount;
						}
						break;
				}

				item.amount = parseFloat(item.amount.toFixed(2));

				items.push(item);
			} else {
				items.push(new AmountByMonth(date, monthString, 0));
			}

			from.setMonth(from.getMonth() + 1);
		}

		switch (type) {
			case TransactionType.Any:
				balanceAverage = balanceSum / monthsDiff;
				spentAverage = Math.abs(spentSum) / monthsDiff;
				depositedAverage = depositedSum / monthsDiff;
				break;
			case TransactionType.Expense:
				spentAverage = Math.abs(spentSum) / monthsDiff;
				break;
			case TransactionType.Deposit:
				depositedAverage = depositedSum / monthsDiff;
				break;
			case TransactionType.Saving:
				savedAverage = savedSum / monthsDiff;
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
		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();
		categoriesService = new CategoriesService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const now = new Date();
		const fromOpts = new Array<FromOption>();
		const date = new Date(now.getFullYear(), now.getMonth(), 1);
		for (let i = 1; i <= 6; i++) {
			date.setMonth(date.getMonth() - 6);

			const value = DateHelper.format(date);
			const label = i * 6 + ' ' + $t('barChartReport.months');

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
			<i class="fas fa-chart-bar" />
		</div>
		<div class="page-title">{$t('barChartReport.barChart')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control inline">
				<label for="from-the-past">{$t('barChartReport.fromThePast')}</label>
				<select id="from-the-past" bind:value={fromDate} on:change={loadData} class="category-select">
					{#if fromOptions}
						{#each fromOptions as from}
							<option value={from.value}>{from.label}</option>
						{/each}
					{/if}
				</select>
			</div>
			<div class="form-control inline">
				<label for="category">{$t('category')}</label>
				<div class="loadable-select" class:loaded={categoryOptions}>
					<select
						id="category"
						bind:value={categoryId}
						on:change={loadData}
						disabled={!categoryOptions}
						class="category-select"
					>
						{#if categoryOptions}
							{#each categoryOptions as category}
								<option value={category.id}>{category.name}</option>
							{/each}
						{/if}
					</select>
					<i class="fas fa-circle-notch fa-spin" />
				</div>
			</div>
			<div class="form-control">
				<div class="multi-radio-wrap">
					{#if categoryType === 0}
						<div class="multi-radio-part">
							<label class:selected={type === 0}>
								<span>{$t('barChartReport.balance')}</span>
								<input type="radio" name="typeToggle" value={0} bind:group={type} on:change={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType !== 1}
						<div class="multi-radio-part">
							<label class:selected={type === 1}>
								<span>{$t('barChartReport.expenses')}</span>
								<input type="radio" name="typeToggle" value={1} bind:group={type} on:change={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType !== 2}
						<div class="multi-radio-part">
							<label class:selected={type === 2}>
								<span>{$t('barChartReport.deposits')}</span>
								<input type="radio" name="typeToggle" value={2} bind:group={type} on:change={loadData} />
							</label>
						</div>
					{/if}

					{#if categoryType === 0}
						<div class="multi-radio-part">
							<label class:selected={type === 4}>
								<span>{$t('barChartReport.savings')}</span>
								<input type="radio" name="typeToggle" value={4} bind:group={type} on:change={loadData} />
							</label>
						</div>
					{/if}
				</div>
			</div>
		</form>

		<div class="bar-chart-wrap">
			<canvas bind:this={canvas} />
		</div>

		<div class="per-month-table-title">{$t('barChartReport.perMonth')}</div>

		<table class="per-month-table">
			{#if type === 0}
				<tr>
					<td>{$t('balance')}</td>
					<td>{Formatter.money(balanceAverage, currency, $locale)}</td>
				</tr>
			{/if}

			{#if type === 0 || type === 1}
				<tr>
					<td>{$t('barChartReport.spent')}</td>
					<td>{Formatter.money(spentAverage, currency, $locale)}</td>
				</tr>
			{/if}

			{#if type === 0 || type === 2}
				<tr>
					<td>{$t('barChartReport.deposited')}</td>
					<td>{Formatter.money(depositedAverage, currency, $locale)}</td>
				</tr>
			{/if}

			{#if type === 4}
				<tr>
					<td>{$t('barChartReport.saved')}</td>
					<td>{Formatter.money(savedAverage, currency, $locale)}</td>
				</tr>
			{/if}
		</table>
	</div>
</section>

<style lang="scss">
	.bar-chart-wrap {
		margin: 30px 0 20px;
	}

	.per-month-table-title {
		display: block;
		border-bottom: 1px solid #ddd;
		padding-bottom: 5px;
		margin: 35px 0 10px;
		text-decoration: none;
		color: var(--primary-color);
	}

	.per-month-table {
		width: 100%;
		font-size: 1rem;

		td {
			padding: 5px 15px 5px 0;
			line-height: 1.2rem;
		}

		td:last-child {
			padding-right: 0;
			text-align: right;
			white-space: nowrap;
		}
	}

	@media screen and (min-width: 1200px) {
		.per-month-table td {
			font-size: 1.1rem;
			line-height: 1.3rem;
		}
	}
</style>
