<script lang="ts">
	import { onMount, onDestroy } from 'svelte';

	import Checkbox from '../../../../../../Core/shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { user, alertState } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { NotificationsServiceBase } from '../../../../../../Core/shared2/services/notificationsServiceBase';
	import { RecipesService } from '$lib/services/recipesService';

	const notificationsVapidKey =
		'BIWWy4ZjIrLMVBxYwsq4rlixA3miMGeMw0yCqldR5Cpv5mozBw1oQxEbbp5q1I9SL9_zUjaLfYheoqb578becPY';
	let notificationsState: string;
	const notificationIconSrc = '/images/icons/app-icon-x96.png';
	let notificationsAreSupported = false;
	let notificationsCheckboxChecked: boolean;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;
	let notificationsService: NotificationsServiceBase;
	let recipesService: RecipesService;

	async function notificationsCheckboxCheckedChanged() {
		const previousNotificationsPermission = (Notification as any).permission;

		if (previousNotificationsPermission === 'denied') {
			alertState.update((x) => {
				x.showError('preferences.notificationsUnpermitted');
				return x;
			});
		} else {
			const permission = await Notification.requestPermission();

			switch (permission) {
				case 'granted':
					const previousNotificationState = notificationsState;

					if (previousNotificationsPermission === 'granted') {
						notificationsState = notificationsCheckboxChecked ? 'checked' : 'unchecked';
					} else {
						notificationsCheckboxChecked = true;
						notificationsState = 'checked';
					}

					if (notificationsCheckboxChecked) {
						try {
							await subscribeToPushNotifications();
						} catch {
							notificationsState = previousNotificationState;
							notificationsCheckboxChecked = false;
						}
					}

					break;
				case 'denied':
					notificationsState = 'denied';
					notificationsCheckboxChecked = false;
					break;
				default:
					notificationsCheckboxChecked = false;
					break;
			}

			await usersService.updateNotificationsEnabled(notificationsCheckboxChecked);
		}
	}

	async function subscribeToPushNotifications() {
		const swReg = await navigator.serviceWorker.ready;

		const sub = await swReg.pushManager.getSubscription();
		if (sub === null) {
			const newSub = await swReg.pushManager.subscribe({
				userVisibleOnly: true,
				applicationServerKey: NotificationsServiceBase.getApplicationServerKey(notificationsVapidKey)
			});

			await notificationsService.createSubscription('Chef', newSub);

			await swReg.showNotification('Chef', {
				body: $t('preferences.notificationsWereEnabled'),
				icon: notificationIconSrc,
				tag: 'notifications-enabled'
			});
		}
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		usersService = new UsersService();
		notificationsService = new NotificationsServiceBase('Chef');
		recipesService = new RecipesService();

		if ('Notification' in window) {
			notificationsAreSupported = true;

			switch ((Notification as any).permission) {
				case 'granted':
					notificationsState = $user.chefNotificationsEnabled ? 'checked' : 'unchecked';
					if ($user.chefNotificationsEnabled) {
						notificationsCheckboxChecked = true;
					}
					break;
				case 'denied':
					notificationsState = 'denied';
					break;
				default:
					notificationsState = 'default';
			}
		}
	});

	onDestroy(() => {
		usersService?.release();
		recipesService?.release();
		notificationsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-sliders-h" />
		</div>
		<div class="page-title">{$t('preferences.preferences')}</div>
		<a href="/recipes" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<form>
			{#if notificationsAreSupported}
				<div class="form-control">
					{#if notificationsState === 'denied'}
						<div class="setting-descriptor no-bottom-margin">
							{$t('preferences.notificationsPermission')}
						</div>
					{/if}

					<Checkbox
						labelKey="preferences.notifications"
						bind:value={notificationsCheckboxChecked}
						disabled={notificationsState === 'denied'}
						on:change={notificationsCheckboxCheckedChanged}
					/>
				</div>
			{/if}
		</form>
	</div>
</section>
