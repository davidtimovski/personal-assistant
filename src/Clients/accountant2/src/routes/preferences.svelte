<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';

	import Checkbox from '$lib/components/Checkbox.svelte';

	let localStorage: LocalStorageUtil;

	let mergeDebtPerPerson: boolean;
	let showUpcomingExpensesOnDashboard: boolean;
	let showDebtOnDashboard: boolean;

	function mergeDebtPerPersonChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.MergeDebtPerPerson, ev.detail);
	}

	function showUpcomingExpensesOnDashboardChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.ShowUpcomingExpensesOnDashboard, ev.detail);
	}

	function showDebtOnDashboardChanged(ev: { detail: boolean }) {
		localStorage.set(LocalStorageKeys.ShowDebtOnDashboard, ev.detail);
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();

		mergeDebtPerPerson = localStorage.getBool(LocalStorageKeys.MergeDebtPerPerson);
		showUpcomingExpensesOnDashboard = localStorage.getBool(LocalStorageKeys.ShowUpcomingExpensesOnDashboard);
		showDebtOnDashboard = localStorage.getBool(LocalStorageKeys.ShowDebtOnDashboard);
	});
</script>

<section>
	<div class="container">
		<div class="au-animate">
			<div class="page-title-wrap">
				<div class="side inactive small">
					<i class="fas fa-sliders-h" />
				</div>
				<div class="page-title">{$t('preferences.preferences')}</div>
				<a href="/" class="back-button">
					<i class="fas fa-times" />
				</a>
			</div>

			<div class="content-wrap">
				<form>
					<div class="form-control">
						<!-- <currency-selector /> -->
					</div>

					<div class="form-control">
						<Checkbox
							labelKey="preferences.mergeDebtPerPerson"
							value={mergeDebtPerPerson}
							on:change={mergeDebtPerPersonChanged}
						/>
					</div>

					<div class="form-control-group">
						<div class="setting-descriptor">{$t('preferences.showOnDashboard')}</div>
						<div class="form-control">
							<Checkbox
								labelKey="preferences.upcomingExpenses"
								value={showUpcomingExpensesOnDashboard}
								on:change={showUpcomingExpensesOnDashboardChanged}
							/>
						</div>
						<div class="form-control">
							<Checkbox
								labelKey="preferences.debt"
								value={showDebtOnDashboard}
								on:change={showDebtOnDashboardChanged}
							/>
						</div>
					</div>
				</form>
			</div>
		</div>
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
