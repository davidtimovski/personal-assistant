<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';

	import Checkbox from '../../../../../shared2/components/Checkbox.svelte';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { user, alertState } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { NotificationsService } from '$lib/services/notificationsService';
	import { ListsService } from '$lib/services/listsService';
	import { SoundPlayer } from '$lib/utils/soundPlayer';

	const notificationsVapidKey =
		'BCL8HRDvXuYjw011VypF_TtfmklYFmqXAADY7pV3WB9vL609d8wNK0zTUs4hB0V3uAnCTpzOd2pANBmsMQoUhD0';
	let notificationsState: string;
	const notificationIconSrc = '/images/icons/app-icon-x96.png';
	let notificationsAreSupported = false;
	let notificationsCheckboxChecked: boolean;
	let soundsEnabled: boolean;
	let highPriorityListEnabled: boolean;
	let staleTasksListEnabled: boolean;

	let localStorage: LocalStorageUtil;
	let usersService: UsersService;
	let notificationsService: NotificationsService;
	let listsService: ListsService;
	let soundPlayer: SoundPlayer;

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
				applicationServerKey: NotificationsService.getApplicationServerKey(notificationsVapidKey)
			});

			await notificationsService.createSubscription('To Do Assistant', newSub);

			await swReg.showNotification('To Do Assistant', {
				body: $t('preferences.notificationsWereEnabled'),
				icon: notificationIconSrc,
				tag: 'notifications-enabled'
			});
		}
	}

	async function soundsEnabledChanged() {
		localStorage.set(LocalStorageKeys.SoundsEnabled, soundsEnabled);

		if (soundsEnabled) {
			soundPlayer.playBleep();
		}
	}

	function highPriorityListEnabledChanged() {
		localStorage.set(LocalStorageKeys.HighPriorityListEnabled, highPriorityListEnabled);
		listsService.getAll();
	}

	function staleTasksListEnabledChanged() {
		localStorage.set(LocalStorageKeys.StaleTasksListEnabled, staleTasksListEnabled);
		listsService.getAll();
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		usersService = new UsersService();
		notificationsService = new NotificationsService();
		listsService = new ListsService();
		soundPlayer = new SoundPlayer();

		if ('Notification' in window) {
			notificationsAreSupported = true;

			switch ((Notification as any).permission) {
				case 'granted':
					notificationsState = $user.toDoNotificationsEnabled ? 'checked' : 'unchecked';
					if ($user.toDoNotificationsEnabled) {
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

		soundsEnabled = localStorage.getBool(LocalStorageKeys.SoundsEnabled);
		highPriorityListEnabled = localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled);
		staleTasksListEnabled = localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled);
	});

	onDestroy(() => {
		usersService?.release();
		listsService?.release();
		notificationsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-sliders-h" />
		</div>
		<div class="page-title">{$t('preferences.preferences')}</div>
		<a href="/lists" class="back-button">
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

			<div class="form-control">
				<Checkbox labelKey="preferences.sounds" bind:value={soundsEnabled} on:change={soundsEnabledChanged} />
			</div>

			<div class="form-control-group">
				<div class="setting-descriptor">{$t('preferences.derivedLists')}</div>
				<div class="form-control">
					<Checkbox
						labelKey="preferences.highPriority"
						bind:value={highPriorityListEnabled}
						on:change={highPriorityListEnabledChanged}
					/>
				</div>

				<div class="form-control">
					<Checkbox
						labelKey="preferences.staleTasks"
						bind:value={staleTasksListEnabled}
						on:change={staleTasksListEnabledChanged}
					/>
				</div>
			</div>
		</form>
	</div>
</section>
