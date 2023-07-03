<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { AuthService } from '../../../../../../Core/shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { NotificationsService } from '$lib/services/notificationsService';
	import { ListsService } from '$lib/services/listsService';
	import Variables from '$lib/variables';

	let unseenNotifications = 0;
	let pendingShareRequestCount = 0;
	let preferencesButtonIsLoading = false;
	const personalAssistantUrl = Variables.urls.account;
	let version = '--';

	let localStorage: LocalStorageUtil;
	let notificationsService: NotificationsService;
	let listsService: ListsService;

	async function goToPreferences() {
		preferencesButtonIsLoading = true;
		await goto('/preferences');
	}

	async function logOut() {
		localStorage.clear();

		const authService = new AuthService();
		await authService.initialize();
		await authService.logout();
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		notificationsService = new NotificationsService();
		listsService = new ListsService();

		notificationsService.getUnseenNotificationsCount().then((unseen) => {
			unseenNotifications = unseen;
		});

		listsService.getPendingShareRequestsCount().then((pending) => {
			pendingShareRequestCount = pending;
		});

		caches.keys().then((cacheNames) => {
			if (cacheNames.length > 0) {
				version = cacheNames.sort().reverse()[0];
			}
		});
	});

	onDestroy(() => {
		notificationsService?.release();
		listsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-bars" />
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href="/lists" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<a href="/notifications" class="wide-button with-badge">
				<span>{$t('menu.notifications')}</span>
				{#if unseenNotifications > 0}
					<span class="badge">{unseenNotifications}</span>
				{/if}
			</a>

			<a href="/archivedLists" class="wide-button">{$t('menu.archivedLists')}</a>

			<a href="/shareRequests" class="wide-button with-badge">
				<span>{$t('menu.shareRequests')}</span>
				{#if pendingShareRequestCount > 0}
					<span class="badge">{pendingShareRequestCount}</span>
				{/if}
			</a>

			<button type="button" on:click={goToPreferences} class="wide-button with-badge">
				<span class="button-loader" class:loading={preferencesButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('menu.preferences')}</span>
			</button>

			<a href="/help" class="wide-button">{$t('menu.help')}</a>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<a href={personalAssistantUrl} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
			<button type="button" on:click={logOut} class="wide-button">{$t('menu.logOut')}</button>
		</div>

		<hr />

		<div class="version"><span>{$t('menu.version')}</span> {version}</div>
	</div>
</section>
