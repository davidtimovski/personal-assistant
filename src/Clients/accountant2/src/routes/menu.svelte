<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { AuthService } from '../../../shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import Variables from '$lib/variables';

	const personalAssistantUrl = Variables.urls.authority;
	let reportsDrawerIsOpen = false;
	let version = '--';

	let preferencesButtonIsLoading = false;
	let helpButtonIsLoading = false;

	function toggleReportsDrawer() {
		reportsDrawerIsOpen = !reportsDrawerIsOpen;
	}

	function goToPreferences() {
		preferencesButtonIsLoading = true;
		goto('preferences');
	}

	function goToHelp() {
		helpButtonIsLoading = true;
		goto('help');
	}

	async function logOut() {
		window.localStorage.removeItem('profileImageUriLastLoad');
		const authService = new AuthService(window);
		await authService.logout();
	}

	onMount(async () => {
		const cacheNames = await caches.keys();
		if (cacheNames.length > 0) {
			version = cacheNames.sort().reverse()[0];
		}
	});
</script>

<section>
	<div class="container">
		<div class="au-animate">
			<div class="page-title-wrap">
				<div class="side inactive">
					<i class="fas fa-bars" />
				</div>
				<div class="page-title">{$t('menu.menu')}</div>
				<a href="/" class="back-button">
					<i class="fas fa-times" />
				</a>
			</div>

			<div class="content-wrap">
				<div class="horizontal-buttons-wrap">
					<!-- <a href="/transactions" class="wide-button">{$tr('menu.transactions')}</a>
					<a href="/upcomingExpenses" class="wide-button">{$tr('menu.upcomingExpenses')}</a>
					<a href="/debt" class="wide-button">{$tr('menu.debt')}</a>
					<a href="/categories" class="wide-button">{$tr('menu.categories')}</a>
					<a href="/accounts" class="wide-button">{$tr('menu.accounts')}</a>
					<a href="/balanceAdjustment" class="wide-button">{$tr('menu.balanceAdjustment')}</a>
					<a href="/automaticTransactions" class="wide-button"
						>{$tr('menu.automaticTransactions')}</a
					>

					<div class="drawer-button-wrap" class:open={reportsDrawerIsOpen}>
						<button type="button" on:click={toggleReportsDrawer} class="wide-button drawer-button">
							{$tr('menu.reports')}
						</button>
						<div class="drawer-content-wrap">
							<div class="drawer-content">
								<a href="/pieChartReport" class="wide-button">{$tr('menu.pieChart')}</a>
								<a href="/barChartReport" class="wide-button">{$tr('menu.barChart')}</a>
								<a href="/expenditureHeatmap" class="wide-button"
									>{$tr('menu.expenditureHeatmap')}</a
								>
							</div>
						</div>
					</div>

					<a href="/earlyRetirementCalculator" class="wide-button"
						>{$tr('menu.earlyRetirementCalculator')}</a
					>
					<a href="/export" class="wide-button">{$tr('menu.export')}</a> -->
					<a href="/totalSync" class="wide-button">{$t('menu.totalSync')}</a>

					<a on:click={goToPreferences} class="wide-button with-badge">
						<span class="button-loader" class:loading={preferencesButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{$t('menu.preferences')}</span>
					</a>

					<!-- <a on:click={goToHelp} class="wide-button with-badge">
						<span class="button-loader" class:loading={helpButtonIsLoading}>
							<i class="fas fa-circle-notch fa-spin" />
						</span>
						<span>{$tr('menu.help')}</span>
					</a> -->
				</div>

				<hr />

				<div class="horizontal-buttons-wrap">
					<a href={personalAssistantUrl} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
					<a on:click={logOut} class="wide-button">{$t('menu.logOut')}</a>
				</div>

				<hr />

				<div class="version"><span>{$t('menu.version')}</span> {version}</div>
			</div>
		</div>
	</div>
</section>

<style lang="scss">
	// .drawer-button-wrap {
	// 	background: #f6f6f6;
	// 	border-radius: 23px;
	// 	box-shadow: inset var(--box-shadow);
	// 	margin-top: 10px;

	// 	.drawer-button {
	// 		box-shadow: none;
	// 		transition: border-radius var(--transition);
	// 	}

	// 	&.open {
	// 		.drawer-button {
	// 			border-bottom: 1px solid #e6e6e6;
	// 			border-bottom-left-radius: 0;
	// 			border-bottom-right-radius: 0;
	// 			color: var(--regular-color);
	// 		}
	// 	}

	// 	.drawer-content-wrap {
	// 		max-height: 0;
	// 		overflow-y: hidden;
	// 		transition-property: all;
	// 		transition-duration: 0.5s;
	// 		transition-timing-function: cubic-bezier(0, 1, 0.5, 1);

	// 		.drawer-content {
	// 			padding: 15px 20px;
	// 		}
	// 	}
	// 	&.open .drawer-content-wrap {
	// 		max-height: 175px;
	// 	}
	// }

	@media screen and (min-width: 1200px) {
		.drawer-button-wrap.open .drawer-content-wrap {
			max-height: 191px;
		}
	}
</style>
