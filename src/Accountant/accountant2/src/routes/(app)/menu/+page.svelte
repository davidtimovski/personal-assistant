<script lang="ts">
	import { onMount } from 'svelte';
	import { resolve } from '$app/paths';

	import { AuthService } from '../../../../../../Core/shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import Variables from '$lib/variables';

	const localStorage = new LocalStorageUtil();

	const personalAssistantUrl = Variables.urls.account;
	let reportsDrawerIsOpen = $state(false);
	let version = $state('--');

	function toggleReportsDrawer() {
		reportsDrawerIsOpen = !reportsDrawerIsOpen;
	}

	async function logOut() {
		localStorage.clear();

		const authService = new AuthService();
		await authService.logout();
	}

	onMount(() => {
		caches.keys().then((cacheNames) => {
			if (cacheNames.length > 0) {
				version = cacheNames.sort().reverse()[0];
			}
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-bars"></i>
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href={resolve('/dashboard')} class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<a data-sveltekit-preload-data="tap" href={resolve('/transactions')} class="wide-button">{$t('menu.transactions')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/upcomingExpenses')} class="wide-button">{$t('menu.upcomingExpenses')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/debt')} class="wide-button">{$t('menu.debt')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/categories')} class="wide-button">{$t('menu.categories')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/accounts')} class="wide-button">{$t('menu.accounts')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/balanceAdjustment')} class="wide-button">{$t('menu.balanceAdjustment')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/automaticTransactions')} class="wide-button">{$t('menu.automaticTransactions')}</a>

			<div class="drawer-button-wrap" class:open={reportsDrawerIsOpen}>
				<button type="button" onclick={toggleReportsDrawer} class="wide-button drawer-button">
					{$t('menu.reports')}
				</button>
				<div class="drawer-content-wrap">
					<div class="drawer-content">
						<a data-sveltekit-preload-data="tap" href={resolve('/pieChartReport')} class="wide-button">{$t('menu.pieChart')}</a>
						<a data-sveltekit-preload-data="tap" href={resolve('/barChartReport')} class="wide-button">{$t('menu.barChart')}</a>
						<a data-sveltekit-preload-data="tap" href={resolve('/expenditureHeatmap')} class="wide-button">{$t('menu.expenditureHeatmap')}</a>
					</div>
				</div>
			</div>

			<a data-sveltekit-preload-data="tap" href={resolve('/earlyRetirementCalculator')} class="wide-button">{$t('menu.earlyRetirementCalculator')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/export')} class="wide-button">{$t('menu.export')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/totalSync')} class="wide-button">{$t('menu.totalSync')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/preferences')} class="wide-button">{$t('menu.preferences')}</a>
			<a data-sveltekit-preload-data="tap" href={resolve('/help')} class="wide-button">{$t('menu.help')}</a>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<a href={resolve(personalAssistantUrl)} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
			<button type="button" onclick={logOut} class="wide-button">{$t('menu.logOut')}</button>
		</div>

		<hr />

		<div class="version"><span>{$t('menu.version')}</span> {version}</div>
	</div>
</section>

<style lang="scss">
	.drawer-button-wrap {
		background: #f6f6f6;
		border-radius: 23px;
		box-shadow: inset var(--box-shadow);
		margin-top: 10px;

		.drawer-button {
			box-shadow: none;
			transition: border-radius var(--transition);
		}

		&.open {
			.drawer-button {
				border-bottom: 1px solid #e6e6e6;
				border-bottom-left-radius: 0;
				border-bottom-right-radius: 0;
				color: var(--regular-color);
			}
		}

		.drawer-content-wrap {
			max-height: 0;
			overflow-y: hidden;
			transition-property: all;
			transition-duration: 0.5s;
			transition-timing-function: cubic-bezier(0, 1, 0.5, 1);

			.drawer-content {
				padding: 15px 20px;
			}
		}
		&.open .drawer-content-wrap {
			max-height: 175px;
		}
	}

	@media screen and (min-width: 1200px) {
		.drawer-button-wrap.open .drawer-content-wrap {
			max-height: 191px;
		}
	}
</style>
