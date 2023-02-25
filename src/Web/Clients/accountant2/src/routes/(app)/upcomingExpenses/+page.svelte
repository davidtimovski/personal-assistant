<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { DateHelper } from '../../../../../shared2/utils/dateHelper';
	import EmptyListMessage from '../../../../../shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { user, syncStatus } from '$lib/stores';
	import { UpcomingExpensesService } from '$lib/services/upcomingExpensesService';
	import { UpcomingExpenseItem } from '$lib/models/viewmodels/upcomingExpenseItem';
	import { SyncEvents } from '$lib/models/syncStatus';

	let upcomingExpenses: UpcomingExpenseItem[] | null = null;
	let currency: string;
	let editedId: number | undefined;

	let localStorage: LocalStorageUtil;
	let upcomingExpensesService: UpcomingExpensesService;

	function formatDate(dateString: string): string {
		const date = new Date(Date.parse(dateString));
		const month = DateHelper.getLongMonth(date, $user.language);

		const now = new Date();
		if (now.getFullYear() === date.getFullYear()) {
			return month;
		}

		return `${month} ${date.getFullYear()}`;
	}

	onMount(async () => {
		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		upcomingExpensesService = new UpcomingExpensesService();

		currency = localStorage.get(LocalStorageKeys.Currency);

		const upcomingExpensesForMonth = await upcomingExpensesService.getAll(currency);

		const upcomingExpenseItems = new Array<UpcomingExpenseItem>();
		for (const upcomingExpense of upcomingExpensesForMonth) {
			upcomingExpenseItems.push(
				new UpcomingExpenseItem(
					upcomingExpense.id,
					upcomingExpense.amount,
					upcomingExpense.currency,
					upcomingExpense.categoryName || $t('uncategorized'),
					formatDate(upcomingExpense.date),
					upcomingExpense.synced
				)
			);
		}

		upcomingExpenses = upcomingExpenseItems;
	});

	onDestroy(() => {
		upcomingExpensesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="far fa-calendar-alt" />
		</div>
		<div class="page-title">{$t('upcomingExpenses.upcomingExpenses')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !upcomingExpenses}
				<div class="double-circle-loading">
					<div class="double-bounce1" />
					<div class="double-bounce2" />
				</div>
			{:else if upcomingExpenses.length > 0}
				<table class="editable-table">
					<thead>
						<tr>
							<th class="edit-link-cell" />
							<th>{$t('upcomingExpenses.month')}</th>
							<th>{$t('amount')}</th>
							<th>{$t('category')}</th>
							<th class="sync-icon-cell" />
						</tr>
					</thead>
					<tbody>
						{#each upcomingExpenses as upcomingExpense}
							<tr class:highlighted-row={upcomingExpense.id === editedId}>
								<td class="edit-link-cell">
									<a
										href="/editUpcomingExpense/{upcomingExpense.id}"
										class="link"
										title={$t('edit')}
										aria-label={$t('edit')}
									>
										<i class="fas fa-pencil-alt" />
									</a>
								</td>
								<td>{upcomingExpense.date}</td>
								<td>{Formatter.number(upcomingExpense.amount, currency, $user.culture)}</td>
								<td>{upcomingExpense.category}</td>
								<td class="sync-icon-cell">
									{#if !upcomingExpense.synced}
										<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')} />
									{/if}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			{:else}
				<EmptyListMessage messageKey="upcomingExpenses.emptyListMessage" />
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				on:click={() => goto('/editUpcomingExpense/0')}
				class="new-button"
				disabled={$syncStatus.status === SyncEvents.SyncStarted}
				title={$t('upcomingExpenses.newUpcomingExpense')}
				aria-label={$t('upcomingExpenses.newUpcomingExpense')}
			>
				<i class="fas fa-plus" />
			</button>
		</div>
	</div>
</section>
