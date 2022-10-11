<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { ArcElement, Chart, PieController } from 'chart.js';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { locale, searchFilters } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { PieChartItem } from '$lib/models/viewmodels/pieChartItem';
	import { SearchFilters } from '$lib/models/viewmodels/searchFilters';
	import { TransactionType } from '$lib/models/viewmodels/transactionType';

	import DoubleRadio from '$lib/components/DoubleRadio.svelte';
	import EmptyListMessage from '$lib/components/EmptyListMessage.svelte';

	let chartPrepared = false;
	let isDeposits = false;
	let mainAccountId: number;
	let currency: string;
	let chart: Chart;
	let items: PieChartItem[] | null = null;
	let sum = 0;
	let canvas: HTMLCanvasElement;
	let canvasCtx: CanvasRenderingContext2D | null = null;
	const colors = ['#7a79e6', '#dbd829', '#49e09b', '#e88042', '#5aacf1', '#f55551', '#b6ca53'];

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;

	const from = new Date();
	from.setDate(1);
	let fromDate = DateHelper.format(from);
	let toDate = DateHelper.format(new Date());
	const maxDate = toDate;

	Chart.register(ArcElement, PieController);
	Chart.defaults.font.family = '"Didact Gothic", sans-serif';

	$: type = isDeposits ? TransactionType.Deposit : TransactionType.Expense;

	async function loadData() {
		chartPrepared = false;
		chart.data.labels = [];
		chart.data.datasets[0].data = [];
		chart.update();
		items = null;

		const byCategory = (
			await transactionsService.getExpendituresAndDepositsByCategory(
				fromDate,
				toDate,
				mainAccountId,
				type,
				currency,
				$t('uncategorized')
			)
		).map((x) => new PieChartItem(x));

		const labels = new Array<string>();
		const amounts = new Array<number>();

		const flatItems = new Array<PieChartItem>();
		const legendColors = [...colors];
		for (const item of byCategory) {
			if (item.subItems.length === 0) {
				item.color = legendColors.length > 0 ? <string>legendColors.shift() : '#e0e0e0';
				flatItems.push(item);

				sum += item.amount;
			} else if (item.subItems.length === 1) {
				item.color = legendColors.length > 0 ? <string>legendColors.shift() : '#e0e0e0';

				const subItem = item.subItems[0];
				item.categoryId = subItem.categoryId;
				item.categoryName += '/' + subItem.categoryName?.replace('- ', '');
				item.subItems = [];
				flatItems.push(item);

				sum += item.amount;
			} else {
				for (const subItem of item.subItems) {
					subItem.color = legendColors.length > 0 ? <string>legendColors.shift() : '#e0e0e0';
					flatItems.push(subItem);

					sum += subItem.amount;
				}
			}
		}

		items = byCategory;

		for (let i = 0; i < flatItems.length; i++) {
			(<any>chart.data.datasets[0]).backgroundColor[i] = flatItems[i].color;

			labels.push(<string>flatItems[i].categoryName);
			amounts.push(flatItems[i].amount);
		}

		if (flatItems.length > 0) {
			chart.data.labels = labels;
			chart.data.datasets[0].data = amounts;
		} else {
			chart.data.labels = [];
			chart.data.datasets[0].data = [];
		}

		chart.update();
	}

	function goToTransactions(item: PieChartItem) {
		searchFilters.set(new SearchFilters(1, 15, fromDate, toDate, 0, item.categoryId, type, null));
		goto('transactions');
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		canvasCtx = canvas.getContext('2d');
		chart = new Chart(<CanvasRenderingContext2D>canvasCtx, {
			type: 'pie',
			data: {
				datasets: [
					{
						data: [],
						backgroundColor: []
					}
				]
			},
			options: {
				animation: {
					onComplete: () => {
						chartPrepared = true;
					}
				}
			}
		});

		mainAccountId = await accountsService.getMainId();
		loadData();
	});

	onDestroy(() => {
		transactionsService?.release();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-chart-pie" />
		</div>
		<div class="page-title">{$t('pieChartReport.pieChart')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control inline">
				<label for="from-date">{$t('pieChartReport.from')}</label>
				<input type="date" id="from-date" bind:value={fromDate} on:change={loadData} />
			</div>
			<div class="form-control inline">
				<label for="to-date">{$t('pieChartReport.to')}</label>
				<input type="date" id="to-date" bind:value={toDate} on:change={loadData} max={maxDate} />
			</div>
			<div class="form-control">
				<DoubleRadio
					name="expensesDepositsToggle"
					leftLabelKey="pieChartReport.expenses"
					rightLabelKey="pieChartReport.deposits"
					bind:value={isDeposits}
					on:change={loadData}
				/>
			</div>
		</form>

		{#if !items}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{/if}

		<div class="pie-chart-wrap">
			<canvas bind:this={canvas} />

			{#if items && items.length === 0}
				<div class="empty-list-message-wrap">
					<EmptyListMessage
						messageKey={isDeposits
							? 'pieChartReport.emptyListMessageDeposits'
							: 'pieChartReport.emptyListMessageExpenses'}
					/>
				</div>
			{/if}
		</div>

		{#if chartPrepared && items}
			{#if items.length > 0}
				<table class="amount-by-category-table au-animate">
					<tbody>
						{#each items as item}
							<tr
								on:click={() => {
									goToTransactions(item);
								}}
							>
								<td>
									{#if item.color}
										<span class="legend-color" style="background: {item.color};" />
									{/if}
									<span>{item.categoryName}</span></td
								>
								<td class="amount-cell">{Formatter.money(item.amount, currency, $locale)}</td>
							</tr>
							{#each item.subItems as subItem}
								<tr
									on:click={() => {
										goToTransactions(subItem);
									}}
								>
									<td class="sub-category-cell">
										<span class="legend-color" style="background: {subItem.color};" />{subItem.categoryName}
									</td>
									<td class="amount-cell">{Formatter.money(subItem.amount, currency, $locale)}</td>
								</tr>
							{/each}
						{/each}
					</tbody>

					{#if items.length > 1}
						<tfoot>
							<tr>
								<td colspan="3">
									<!-- Space taker -->
								</td>
							</tr>
							<tr>
								<td colspan="3">{Formatter.money(sum, currency, $locale)}</td>
							</tr>
						</tfoot>
					{/if}
				</table>
			{/if}
		{/if}
	</div>
</section>

<style lang="scss">
	.pie-chart-wrap {
		position: relative;
		padding: 20px 15%;

		.empty-list-message-wrap {
			position: absolute;
			top: 20%;
			left: 0;
			right: 0;
		}
	}
	.legend-color {
		display: inline-block;
		width: 14px;
		height: 14px;
		border-radius: 4px;
		margin-right: 8px;
		vertical-align: bottom;
	}
</style>
