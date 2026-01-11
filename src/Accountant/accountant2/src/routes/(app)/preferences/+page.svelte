<script lang="ts">
	import { onMount } from 'svelte';

	import CurrencySelector from '../../../../../../Core/shared2/components/CurrencySelector.svelte';
	import Checkbox from '../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';

	const localStorage = new LocalStorageUtil();

	let mergeDebtPerPerson = $state(false);
	let showBalanceOnHomePage = $state(false);
	let showUpcomingExpensesOnHomePage = $state(false);
	let showDebtOnHomePage = $state(false);

	function mergeDebtPerPersonChanged() {
		localStorage.set(LocalStorageKeys.MergeDebtPerPerson, mergeDebtPerPerson);
	}

	function showBalanceOnHomePageChanged() {
		localStorage.set(LocalStorageKeys.ShowBalanceOnHomePage, showBalanceOnHomePage);
	}

	function showUpcomingExpensesOnHomePageChanged() {
		localStorage.set(LocalStorageKeys.ShowUpcomingExpensesOnHomePage, showUpcomingExpensesOnHomePage);
	}

	function showDebtOnHomePageChanged() {
		localStorage.set(LocalStorageKeys.ShowDebtOnHomePage, showDebtOnHomePage);
	}

	onMount(() => {
		mergeDebtPerPerson = localStorage.getBool(LocalStorageKeys.MergeDebtPerPerson);
		showBalanceOnHomePage = localStorage.getBool(LocalStorageKeys.ShowBalanceOnHomePage);
		showUpcomingExpensesOnHomePage = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnHomePage);
		showDebtOnHomePage = localStorage.getBool(LocalStorageKeys.ShowDebtOnHomePage);
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-sliders-h"></i>
		</div>
		<div class="page-title">{$t('preferences.preferences')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control">
				<CurrencySelector application="Accountant" />
			</div>

			<div class="form-control">
				<Checkbox labelKey="preferences.mergeDebtPerPerson" bind:value={mergeDebtPerPerson} onchange={mergeDebtPerPersonChanged} />
			</div>

			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.showOnHomePage')}</div>

				<div class="form-control">
					<Checkbox labelKey="balance" bind:value={showBalanceOnHomePage} onchange={showBalanceOnHomePageChanged} />
				</div>

				<div class="form-control">
					<Checkbox
						labelKey="preferences.upcomingExpenses"
						bind:value={showUpcomingExpensesOnHomePage}
						onchange={showUpcomingExpensesOnHomePageChanged}
					/>
				</div>

				<div class="form-control">
					<Checkbox labelKey="preferences.debt" bind:value={showDebtOnHomePage} onchange={showDebtOnHomePageChanged} />
				</div>
			</div>
		</form>
	</div>
</section>

<style lang="scss">
	.form-control-group {
		margin-top: 20px;

		.setting-descriptor {
			margin-bottom: 5px;
		}

		.form-control {
			padding-bottom: 0;
		}
	}
</style>
