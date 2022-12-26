<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import Tooltip from '../../../../../shared2/components/Tooltip.svelte';
	import EmptyListMessage from '../../../../../shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { ListsService } from '$lib/services/listsService';
	import type { ShareRequest } from '$lib/models/viewmodels/shareRequest';

	let pendingShareRequests: Array<ShareRequest> | null = null;
	let declinedShareRequests: Array<ShareRequest> | null = null;

	let listsService: ListsService;

	async function accept(request: ShareRequest) {
		if (pendingShareRequests === null) {
			throw new Error('Unexpected error: required fields missing');
		}

		await listsService.setShareIsAccepted(request.listId, true);

		await goto(`/?edited=${request.listId}`);
	}

	async function decline(request: ShareRequest) {
		if (pendingShareRequests === null || declinedShareRequests === null) {
			throw new Error('Unexpected error: required fields missing');
		}

		await listsService.setShareIsAccepted(request.listId, false);
		pendingShareRequests = pendingShareRequests.filter((x) => x.listId !== request.listId);
		declinedShareRequests = [request, ...declinedShareRequests];
	}

	onMount(async () => {
		listsService = new ListsService();

		const allShareRequests = await listsService.getShareRequests();

		pendingShareRequests = allShareRequests.filter((request: ShareRequest) => {
			return request.isAccepted === null;
		});
		declinedShareRequests = allShareRequests.filter((request: ShareRequest) => {
			return request.isAccepted === false;
		});
	});

	onDestroy(() => {
		listsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-handshake" />
		</div>
		<div class="page-title">{$t('shareRequests.shareRequests')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !pendingShareRequests || !declinedShareRequests}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else if pendingShareRequests.length === 0 && declinedShareRequests.length === 0}
			<EmptyListMessage messageKey="shareRequests.emptyListMessage" />
		{/if}

		{#if pendingShareRequests && pendingShareRequests.length > 0}
			<div class="share-requests-wrap">
				<Tooltip key="shareRequests" application="To Do Assistant" />

				{#each pendingShareRequests as request}
					<div class="share-request">
						<button
							type="button"
							on:click={() => decline(request)}
							class="action"
							title={$t('shareRequests.decline')}
							aria-label={$t('shareRequests.decline')}
						>
							<i class="fas fa-ban" />
						</button>

						<span class="name">
							<span class="colored-text">{request.listName}</span>
							<span>{$t('shareRequests.by')}</span>
							<span class="colored-text">{request.listOwnerName}</span>
						</span>

						<button
							type="button"
							on:click={() => accept(request)}
							class="action"
							title={$t('shareRequests.accept')}
							aria-label={$t('shareRequests.accept')}
						>
							<i class="fas fa-check" />
						</button>
					</div>
				{/each}
			</div>
		{/if}

		{#if declinedShareRequests && declinedShareRequests.length > 0}
			<div class="labeled-separator">
				<div class="labeled-separator-text">{$t('shareRequests.declinedRequests')}</div>
				<hr />
			</div>

			<div class="share-requests-wrap declined">
				{#each declinedShareRequests as request}
					<div class="share-request">
						<div class="action inactive" />

						<span class="name">
							<span class="colored-text">{request.listName}</span>
							<span>{$t('shareRequests.by')}</span>
							<span class="colored-text">{request.listOwnerName}</span>
						</span>

						<button
							type="button"
							on:click={() => accept(request)}
							class="action"
							title={$t('shareRequests.accept')}
							aria-label={$t('shareRequests.accept')}
						>
							<i class="fas fa-check" />
						</button>
					</div>
				{/each}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.share-requests-wrap {
		&.declined {
			margin-top: 15px;

			.share-request {
				opacity: 0.6;

				&:hover {
					opacity: 1;
				}
			}
		}

		.share-request {
			display: flex;
			justify-content: space-between;
			background: #e9f4ff;
			border-radius: 6px;
			margin-bottom: 10px;

			.action {
				min-width: 45px;
				background: transparent;
				border: none;
				outline: none;
				font-size: 27px;
				line-height: 45px;
				text-decoration: none;
				text-align: center;
				color: var(--primary-color);

				&.inactive,
				&.inactive:hover {
					color: var(--faded-color);
				}

				&:not(.inactive):not(.side-loading):hover {
					color: var(--primary-color-dark);
				}
			}

			.name {
				width: 100%;
				padding: 9px 5px;
				line-height: 27px;
				text-align: center;
				cursor: default;
			}
		}
	}
</style>
