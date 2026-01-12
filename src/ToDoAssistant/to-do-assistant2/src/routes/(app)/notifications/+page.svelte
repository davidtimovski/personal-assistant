<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { Unsubscriber } from 'svelte/store';

	import EmptyListMessage from '../../../../../../Core/shared2/components/EmptyListMessage.svelte';
	import { DateHelper } from '../../../../../../Core/shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { authInfo, user } from '$lib/stores';
	import { NotificationsService } from '$lib/services/notificationsService';
	import type { Notification } from '$lib/models/viewmodels/notification';

	let highlightedId: number | undefined = $state(undefined);
	let unseenNotifications: Array<Notification> | null = $state(null);
	let seenNotifications: Array<Notification> | null = $state(null);
	let seenNotificationsVisible = $state(false);
	const unsubscriptions: Unsubscriber[] = [];

	let notificationsService: NotificationsService;

	function showSeenNotifications() {
		seenNotificationsVisible = true;
	}

	function replacePlaceholders(notification: Notification) {
		notification.message = notification.message.replace(/#\[/g, '<span class="colored-text">').replace(/\]#/g, '</span>');
		return notification;
	}

	onMount(async () => {
		const url = new URL(window.location.href);
		const queryParams = new URLSearchParams(url.search);
		const id = queryParams.get('id');
		if (id) {
			highlightedId = parseInt(id, 10);
		}

		notificationsService = new NotificationsService();

		unsubscriptions.push(
			authInfo.subscribe(async (value) => {
				if (!value) {
					return;
				}

				const allNotifications = await notificationsService.getAll();

				for (const notification of allNotifications) {
					notification.formattedCreatedDate = DateHelper.formatWeekdayTime(new Date(notification.createdDate), $user.language, $user.culture);
					if (notification.listId && notification.taskId) {
						notification.url = `/list/${notification.listId}?edited=${notification.taskId}`;
					} else if (notification.listId) {
						notification.url = `/list/${notification.listId}`;
					} else {
						notification.url = '#';
					}
				}

				unseenNotifications = allNotifications
					.filter((notification) => {
						return !notification.isSeen;
					})
					.map(replacePlaceholders);
				seenNotifications = allNotifications
					.filter((notification) => {
						return notification.isSeen;
					})
					.map(replacePlaceholders);
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		notificationsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-bell"></i>
		</div>
		<div class="page-title">{$t('notifications.notifications')}</div>
		<a href="/lists" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		{#if !unseenNotifications || !seenNotifications}
			<div class="double-circle-loading">
				<div class="double-bounce1"></div>
				<div class="double-bounce2"></div>
			</div>
		{:else if unseenNotifications.length === 0 && seenNotifications.length === 0}
			<EmptyListMessage messageKey="notifications.emptyListMessage" />
		{/if}

		{#if unseenNotifications && unseenNotifications.length > 0}
			<div class="notifications-wrap">
				{#each unseenNotifications as notification}
					<a href={notification.url} class="notification" class:highlighted={notification.id === highlightedId}>
						<img
							class="notification-image"
							src={notification.userImageUri}
							title={notification.userName}
							alt={$t('profilePicture', { name: notification.userName })}
						/>
						<div class="notification-content">
							<div class="name" contenteditable="false" bind:innerHTML={notification.message}></div>
							<div class="notification-time">{notification.formattedCreatedDate}</div>
						</div>
					</a>
				{/each}
			</div>
		{/if}

		{#if seenNotifications && seenNotifications.length > 0}
			<div>
				<div class="centering-wrap">
					{#if !seenNotificationsVisible}
						<button type="button" onclick={showSeenNotifications} class="show-button">
							{$t('notifications.showSeen')}
						</button>
					{/if}
				</div>

				{#if seenNotificationsVisible}
					<div class="labeled-separator">
						<div class="labeled-separator-text">{$t('notifications.seen')}</div>
						<hr />
					</div>

					<div class="notifications-wrap seen">
						{#each seenNotifications as notification}
							<a href={notification.url} class="notification" class:highlighted={notification.id === highlightedId}>
								<img
									class="notification-image"
									src={notification.userImageUri}
									title={notification.userName}
									alt={$t('profilePicture', { name: notification.userName })}
								/>
								<div class="notification-content">
									<div class="name" contenteditable="false" bind:innerHTML={notification.message}></div>
									<div class="notification-time">{notification.formattedCreatedDate}</div>
								</div>
							</a>
						{/each}
					</div>
				{/if}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.notifications-wrap.seen {
		margin-top: 15px;
	}

	.notification {
		display: block;
		background: #f4faff;
		border-radius: 8px;
		padding: 9px 10px;
		margin-bottom: 10px;
		text-decoration: none;
		color: var(--regular-color);

		&-image {
			width: 34px;
			height: 34px;
			border-radius: 50%;
			vertical-align: top;
		}

		&-content {
			display: inline-block;
			width: calc(100% - 55px);
			padding-left: 15px;
		}

		.name {
			line-height: 27px;
		}

		&-time {
			margin-top: 15px;
			font-size: 16px;
			line-height: 20px;
			text-align: right;
			color: var(--faded-color);
		}
	}

	.show-button {
		background: transparent;
		border: none;
		outline: none;
		padding: 5px 15px;
		margin-top: 8%;
		font-size: 1.2rem;
		line-height: 27px;
		text-decoration: underline;
		color: var(--primary-color);
	}
</style>
