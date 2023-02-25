<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { t } from '$lib/localization/i18n';
	import { Formatter } from '$lib/utils/formatter';
	import { user } from '$lib/stores';
	import { TransactionsService } from '$lib/services/transactionsService';
	import { AccountsService } from '$lib/services/accountsService';
	import { HeatmapDay, HeatmapExpense } from '$lib/models/viewmodels/expenditureHeatmap';

	let days: HeatmapDay[] | null = null;
	let selectedDay: HeatmapDay | null = null;
	let selectedExpenditureCaret = 0;
	let loaded = false;
	const colors = [
		'#241432',
		'#2b1637',
		'#33183c',
		'#381a40',
		'#401b44',
		'#481c48',
		'#4e1d4b',
		'#561e4f',
		'#5c1e51',
		'#641f54',
		'#6d1f56',
		'#731f58',
		'#7b1f59',
		'#841e5a',
		'#8b1d5b',
		'#931c5b',
		'#9a1b5b',
		'#a3195b',
		'#ab185a',
		'#b21758',
		'#ba1656',
		'#c11754',
		'#c81951',
		'#cf1e4d',
		'#d5224a',
		'#db2946',
		'#e03143',
		'#e43841',
		'#e8403e',
		'#eb483e',
		'#ee523f',
		'#f05c42',
		'#f16445',
		'#f26d4b',
		'#f37450',
		'#f47d57',
		'#f4865e',
		'#f58d64',
		'#f5966c',
		'#f69e75',
		'#f6a47c',
		'#f6ad85',
		'#f6b38d',
		'#f6bb97',
		'#f7c2a2',
		'#f7c9aa',
		'#f7d0b5',
		'#f8d7c0',
		'#f9ddc9',
		'#f9e5d4'
	];
	let currency: string;

	const now = new Date();
	const month = now.getMonth() - 1;
	const aMonthAgo = new Date(now.getFullYear(), month, now.getDate());
	const weekday = aMonthAgo.getDay();

	if (weekday > 1 || weekday === 0) {
		const diff = weekday - 1;
		aMonthAgo.setDate(aMonthAgo.getDate() - diff);
	}

	const fromDate = new Date(aMonthAgo.getFullYear(), aMonthAgo.getMonth(), aMonthAgo.getDate());
	const todayString = DateHelper.format(now);

	let maxSpent = 0;
	let minSpent = 10000000;

	let localStorage: LocalStorageUtil;
	let transactionsService: TransactionsService;
	let accountsService: AccountsService;

	function formatDate(date: Date): string {
		const day = date.getDate();
		const month = DateHelper.getLongMonth(date, $user.language);
		return `${day} ${month}`;
	}

	function formatDescription(description: string | null, isEncrypted: boolean): string {
		if (isEncrypted) {
			return $t('encryptedPlaceholder');
		}

		if (!description) {
			return '';
		}

		const length = 25;
		if (description.length <= length) {
			return description;
		}

		return description.substring(0, length - 2) + '..';
	}

	function select(day: HeatmapDay) {
		selectedDay = day;
		selectedExpenditureCaret = day.spentPercentage;
	}

	function viewTransaction(id: number) {
		goto(`/transaction/${id}?fromExpenditureHeatmap=true`);
	}

	$: selectedDayDate = selectedDay?.date;

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		transactionsService = new TransactionsService();
		accountsService = new AccountsService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const daysArray = new Array<HeatmapDay>();

		while (aMonthAgo < now) {
			for (let i = 0; i < 7; i++) {
				const dateString = DateHelper.format(aMonthAgo);
				const isToday = dateString === todayString;

				const day = new HeatmapDay(
					aMonthAgo.getDate(),
					dateString,
					formatDate(aMonthAgo),
					isToday,
					0,
					0,
					null,
					null,
					[]
				);

				daysArray.push(day);

				if (isToday) {
					selectedDay = day;
				}

				aMonthAgo.setDate(aMonthAgo.getDate() + 1);
			}
		}

		days = daysArray;

		const mainAccountId = await accountsService.getMainId();
		const transactions = await transactionsService.getExpendituresFrom(mainAccountId, fromDate, currency);

		for (const day of daysArray) {
			transactions.forEach((x) => {
				if (day.date === x.date.slice(0, 10)) {
					const categoryName = x.category ? x.category.fullName : $t('uncategorized');
					const trimmedDescription = formatDescription(x.description, x.isEncrypted);

					day.spent += x.amount;
					day.expenditures.push(new HeatmapExpense(x.id, categoryName, trimmedDescription, x.amount));
				}
			});

			if (day.spent > maxSpent) {
				maxSpent = day.spent;
			}
			if (day.spent < minSpent) {
				minSpent = day.spent;
			}
		}

		for (let day of daysArray) {
			day.spentPercentage = (day.spent / maxSpent) * 100;

			if (day.spent === 0) {
				day.backgroundColor = colors[0];
				day.textColor = '#eee';
			} else if (day.spent === maxSpent) {
				day.backgroundColor = colors[49];
				day.textColor = 'initial';
			} else {
				const index = Math.floor(day.spentPercentage / 2);
				day.backgroundColor = colors[index];
				day.textColor = index < 30 ? '#eee' : 'initial';
			}
		}

		days = days.slice(0); // Force days color update
		maxSpent = maxSpent;
		minSpent = minSpent;
		loaded = true;
	});

	onDestroy(() => {
		transactionsService?.release();
		accountsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-th" />
		</div>
		<div class="page-title">{$t('expenditureHeatmap.expenditureHeatmap')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="heatmap" class:loaded>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.mon')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.tue')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.wed')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.thu')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.fri')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.sat')}</div>
			<div class="heatmap-cell header">{$t('expenditureHeatmap.sun')}</div>

			{#if days}
				{#each days as day}
					<button
						type="button"
						on:click={() => select(day)}
						class="heatmap-cell date"
						class:today={day.isToday}
						class:selected={day.date === selectedDayDate}
						style="background: {day.backgroundColor}; color: {day.textColor};"
					>
						{day.day}
					</button>
				{/each}
			{/if}
		</div>

		{#if loaded}
			<div>
				<div class="heatmap-legend">
					<div style="width: {selectedExpenditureCaret}%;" class="heatmap-caret-wrap">
						<i class="fas fa-caret-down" />
					</div>
					<div class="heatmap-legend-line" />
					<div class="heatmap-legend-amounts">
						<span>{Formatter.number(minSpent, currency, $user.culture)}</span>
						<span>{Formatter.number(maxSpent, currency, $user.culture)}</span>
					</div>
				</div>

				{#if selectedDay}
					<div class="expenditure-heatmap-table-title">{selectedDay.formattedDate}</div>
					<table class="expenditure-heatmap-table">
						<tbody>
							{#each selectedDay.expenditures as expenditure}
								<tr on:click={() => viewTransaction(expenditure.transactionId)} role="button">
									<td>{expenditure.category}</td>
									<td>{expenditure.description}</td>
									<td class="amount-cell">{Formatter.money(expenditure.amount, currency, $user.culture)}</td>
								</tr>
							{/each}
						</tbody>

						{#if selectedDay.expenditures.length > 1}
							<tfoot>
								<tr>
									<td colspan="3">
										<!-- Space taker -->
									</td>
								</tr>
								<tr>
									<td colspan="3">{Formatter.money(selectedDay.spent, currency, $user.culture)}</td>
								</tr>
							</tfoot>
						{/if}
					</table>
				{/if}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.heatmap {
		display: grid;
		grid-template-columns: 1fr 1fr 1fr 1fr 1fr 1fr 1fr;
		column-gap: 3px;
		row-gap: 3px;
		font-size: 1.15rem;
		line-height: 2.1rem;

		.heatmap-cell {
			outline: none;
			padding: 6px;
			text-align: center;
			color: #999;
			transition: color var(--transition);

			&.header {
				font-size: 1rem;
			}

			&.date {
				background: linear-gradient(-90deg, #ddd 0%, #f3f3f3 100%);
				background-size: 400% 400%;
				border: 4px solid transparent;
				animation: heatmapCellLoading 2.5s ease-in-out infinite;
			}

			&.today {
				border-color: var(--green-color-dark);
				padding: 0;
			}

			&.selected {
				border-color: var(--primary-color);
				padding: 0;
			}

			&:nth-child(8) {
				border-top-left-radius: 8px;
			}

			&:nth-child(14) {
				border-top-right-radius: 8px;
			}

			&:nth-last-child(7) {
				border-bottom-left-radius: 8px;
			}

			&:last-child {
				border-bottom-right-radius: 8px;
			}
		}

		&.loaded .heatmap-cell.header {
			color: var(--regular-color);
		}

		&.loaded .date {
			animation: unset;
			background-size: initial;
			transition: background var(--transition), border var(--transition);
		}
	}

	.heatmap-legend {
		position: relative;
		margin-top: 25px;

		&-line {
			height: 10px;
			background: linear-gradient(
				to right,
				#241432 0%,
				#6d1f56 20%,
				#ba1656 40%,
				#ee523f 60%,
				#f6a47c 80%,
				#f9e5d4 100%
			);
		}

		&-amounts {
			display: flex;
			justify-content: space-between;
		}

		.heatmap-caret-wrap {
			position: absolute;
			top: -18px;
			left: 0;
			text-align: right;
			transition: width 400ms ease-out;
		}
	}

	.expenditure-heatmap-table-title {
		display: block;
		border-bottom: 1px solid #ddd;
		padding-bottom: 5px;
		margin: 35px 0 10px;
		text-decoration: none;
		color: var(--primary-color);
	}
	.expenditure-heatmap-table {
		width: 100%;
		font-size: 1rem;

		tbody tr {
			cursor: pointer;

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		td {
			padding: 5px 15px 5px 0;
			line-height: 1.2rem;

			&:last-child {
				padding-right: 0;
				text-align: right;
			}
		}

		.amount-cell {
			white-space: nowrap;
		}

		tfoot tr:last-child td {
			border-top: 1px solid #ddd;
			padding: 5px 0;
			font-size: 1.1rem;
		}
	}

	@keyframes heatmapCellLoading {
		0% {
			background-position: 0% 0%;
		}
		100% {
			background-position: -100% 0%;
		}
	}
</style>
