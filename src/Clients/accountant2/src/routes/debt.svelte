<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';

	import { DateHelper } from '../../../shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { Formatter } from '$lib/utils/formatter';
	import { syncStatus } from '$lib/stores';
	import { DebtsService } from '$lib/services/debtsService';
	import { DebtItem } from '$lib/models/viewmodels/debtItem';
	import { AppEvents } from '$lib/models/appEvents';

	import EmptyListMessage from '$lib/components/EmptyListMessage.svelte';

	let debts: DebtItem[] | null = null;
	let currency: string;
	let language: string;
	let editedId: number | undefined;
	let syncing = false;

	let localStorage: LocalStorageUtil;
	let debtsService: DebtsService;

	function formatDate(dateString: string): string {
		const date = new Date(Date.parse(dateString));
		const month = DateHelper.getLongMonth(date, language);

		const now = new Date();
		if (now.getFullYear() === date.getFullYear()) {
			return month;
		}

		return `${month} ${date.getFullYear()}`;
	}

	function settleDebt(id: number, userIsDebtor: boolean) {
		const type = userIsDebtor ? 0 : 1;
		goto(`newTransaction/${type}?debtId=${id}`);
	}

	onMount(async () => {
		syncStatus.subscribe((value) => {
			if (value === AppEvents.SyncStarted) {
				syncing = true;
			} else if (value === AppEvents.SyncFinished) {
				syncing = false;
			}
		});

		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		localStorage = new LocalStorageUtil();
		debtsService = new DebtsService();

		currency = localStorage.get('currency');
		language = localStorage.get('language');

		const debtDtos = await debtsService.getAll(currency);

		const debtItems = new Array<DebtItem>();
		for (const debtDto of debtDtos) {
			debtItems.push(
				new DebtItem(
					debtDto.id,
					debtDto.userIsDebtor,
					debtDto.amount,
					debtDto.currency,
					debtDto.person,
					formatDate((<Date>debtDto.createdDate).toString()),
					debtDto.synced
				)
			);
		}

		debts = debtItems;
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive medium">
			<i class="fas fa-hand-holding-usd" />
		</div>
		<div class="page-title">{$t('debt.debt')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="content-body">
			{#if !debts}
				<div class="double-circle-loading">
					<div class="double-bounce1" />
					<div class="double-bounce2" />
				</div>
			{:else if debts.length > 0}
				<table class="editable-table">
					<thead>
						<tr>
							<th class="edit-link-cell" />
							<th class="edit-link-cell" />
							<th>{$t('amount')}</th>
							<th>{$t('debt.person')}</th>
							<th>{$t('debt.created')}</th>
							<th class="sync-icon-cell" />
						</tr>
					</thead>
					<tbody>
						{#each debts as debt}
							<tr class:highlighted-row={debt.id === editedId}>
								<td class="edit-link-cell">
									<a href="/editDebt/{debt.id}" class="link" title={$t('edit')} aria-label={$t('edit')}>
										<i class="fas fa-pencil-alt" />
									</a>
								</td>
								<td class="edit-link-cell">
									<button
										type="button"
										on:click={() => settleDebt(debt.id, debt.userIsDebtor)}
										class="settle-debt-button"
										title={$t('debt.settleDebt')}
										aria-label={$t('debt.settleDebt')}
									>
										<i class="fas fa-hand-holding-usd {debt.userIsDebtor ? 'debtor' : 'lender'}" />
									</button>
								</td>
								<td>{Formatter.number(debt.amount, currency)}</td>
								<td>{debt.person}</td>
								<td>{debt.created}</td>
								<td class="sync-icon-cell">
									{#if !debt.synced}
										<i class="fas fa-sync-alt" title={$t('notSynced')} aria-label={$t('notSynced')} />
									{/if}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			{:else}
				<EmptyListMessage messageKey="debt.emptyListMessage" />
			{/if}
		</div>

		<div class="centering-wrap">
			<button
				type="button"
				on:click={() => goto('/editDebt/0')}
				class="new-button"
				disabled={syncing}
				title={$t('debt.newDebt')}
				aria-label={$t('debt.newDebt')}
			>
				<i class="fas fa-plus" />
			</button>
		</div>
	</div>
</section>

<style lang="scss">
	.settle-debt-button {
		display: block;
		background: transparent;
		border: none;
		outline: none;
		padding: 5px;
		font-size: 1.4rem;
		color: var(--primary-color);

		.debtor {
			color: var(--expense-color);
		}

		.lender {
			color: var(--deposit-color);
		}
	}
</style>
