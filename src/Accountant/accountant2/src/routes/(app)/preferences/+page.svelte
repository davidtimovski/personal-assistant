<script lang="ts">
	import { onMount } from 'svelte';

	import CurrencySelector from '../../../../../../Core/shared2/components/CurrencySelector.svelte';
	import Checkbox from '../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';

	let localStorage: LocalStorageUtil;

	let mergeDebtPerPerson: boolean;
	let showUpcomingExpensesOnHomePage: boolean;
	let showDebtOnHomePage: boolean;

	function mergeDebtPerPersonChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.MergeDebtPerPerson, ev.detail);
	}

	function showUpcomingExpensesOnHomePageChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.ShowUpcomingExpensesOnHomePage, ev.detail);
	}

	function showDebtOnHomePageChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.ShowDebtOnHomePage, ev.detail);
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();

		mergeDebtPerPerson = localStorage.getBool(LocalStorageKeys.MergeDebtPerPerson);
		showUpcomingExpensesOnHomePage = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnHomePage);
		showDebtOnHomePage = localStorage.getBool(LocalStorageKeys.ShowDebtOnHomePage);
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-sliders-h" />
		</div>
		<div class="page-title">{$t('preferences.preferences')}</div>
		<a href="/dashboard" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form>
			<div class="form-control">
				<CurrencySelector application="Accountant" />
			</div>

			<div class="form-control">
				<Checkbox
					labelKey="preferences.mergeDebtPerPerson"
					value={mergeDebtPerPerson}
					on:change={mergeDebtPerPersonChanged}
				/>
			</div>

			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.showOnHomePage')}</div>
				<div class="form-control">
					<Checkbox
						labelKey="preferences.upcomingExpenses"
						value={showUpcomingExpensesOnHomePage}
						on:change={showUpcomingExpensesOnHomePageChanged}
					/>
				</div>
				<div class="form-control">
					<Checkbox labelKey="preferences.debt" value={showDebtOnHomePage} on:change={showDebtOnHomePageChanged} />
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
