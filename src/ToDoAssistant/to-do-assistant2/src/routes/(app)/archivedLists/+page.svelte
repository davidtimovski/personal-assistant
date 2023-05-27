<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { page } from '$app/stores';

	import EmptyListMessage from '../../../../../../Core/shared2/components/EmptyListMessage.svelte';

	import { t } from '$lib/localization/i18n';
	import { state } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import type { ArchivedList } from '$lib/models/viewmodels/archivedList';
	import type { ListIcon } from '$lib/models/viewmodels/listIcon';

	let archivedLists: Array<ArchivedList> | null = null;
	const iconOptions = ListsService.getIconOptions();
	let editedId: number | undefined;

	function getClassFromIcon(icon: string): string {
		return (<ListIcon>iconOptions.find((x) => x.icon === icon)).cssClass;
	}

	onMount(async () => {
		const edited = $page.url.searchParams.get('edited');
		if (edited) {
			editedId = parseInt(edited, 10);
		}

		return state.subscribe((s) => {
			if (s.lists === null) {
				return;
			}

			archivedLists = ListsService.getArchived(s.lists);
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-archive" />
		</div>
		<div class="page-title">{$t('archivedLists.archivedLists')}</div>
		<a href="/lists" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !archivedLists}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			{#each archivedLists as list}
				<div
					class="to-do-list"
					class:is-shared={list.sharingState !== 0 && list.sharingState !== 1}
					class:pending-share={list.sharingState === 1}
				>
					<i class="icon {getClassFromIcon(list.icon)}" />
					<a href="/list/{list.id}" class="name" class:highlighted={list.id === editedId}>{list.name}</a>
					<i class="fas fa-users shared-icon" title={$t('index.shared')} aria-label={$t('index.shared')} />
					<i
						class="fas fa-user-clock shared-icon"
						title={$t('index.pendingAccept')}
						aria-label={$t('index.pendingAccept')}
					/>
				</div>
			{/each}
		{/if}

		{#if archivedLists?.length === 0}
			<EmptyListMessage messageKey="archivedLists.emptyListMessage" />
		{/if}
	</div>
</section>

<style lang="scss">
	.to-do-list {
		position: relative;
		display: flex;
		justify-content: flex-start;
		margin: 12px 0;
		user-select: none;

		&:first-child {
			margin-top: 0;
		}

		&:last-child {
			margin-bottom: 0;
		}

		.icon {
			min-width: 43px;
			height: 41px;
			border: 2px solid #9df;
			border-radius: 6px;
			margin-right: 8px;
			line-height: 41px;
			text-align: center;
			font-size: 22px;
			color: var(--primary-color);
		}

		.shared-icon {
			display: none;
			position: absolute;
			top: 11px;
			right: 10px;
			font-size: 24px;
			color: var(--primary-color);
		}

		&.is-shared,
		&.pending-share {
			padding-right: 50px;
		}

		&.is-shared .fa-users {
			display: inline;
		}

		&.pending-share .fa-user-clock {
			display: inline;
		}

		.name {
			width: 100%;
			background: #e9f4ff;
			border-radius: 6px;
			padding: 8px 15px;
			line-height: 29px;
			font-size: 1.1rem;
			text-decoration: none;
			color: var(--regular-color);

			&:hover {
				background: #e1f0ff;
			}
		}

		&.is-shared .name {
			margin-right: 8px;
		}
	}
</style>
