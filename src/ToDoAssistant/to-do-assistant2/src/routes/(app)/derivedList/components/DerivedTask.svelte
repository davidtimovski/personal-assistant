<script lang="ts">
	import { DateHelper } from '../../../../../../../Core/shared2/utils/dateHelper';

	import { t } from '$lib/localization/i18n';
	import { user } from '$lib/stores';
	import type { Assignee } from '$lib/models/viewmodels/assignee';

	let {
		active,
		disabled,
		highPriority,
		stale = false,
		assignedUser = null,
		name,
		url,
		isOneTime,
		modifiedDate,
		onclick
	}: {
		active: boolean;
		disabled: boolean;
		highPriority: boolean;
		stale: boolean;
		assignedUser?: Assignee | null;
		name: string;
		url: string | null;
		isOneTime: boolean;
		modifiedDate: string;
		onclick: any;
	} = $props();

	const formattedStaleTaskDate = $derived(DateHelper.formatDayMonth(new Date(modifiedDate), $user.language));
</script>

<div class="to-do-task" class:active class:high-priority={highPriority}>
	{#if assignedUser}
		<img
			src={assignedUser.imageUri}
			class="to-do-task-assignee-image"
			title={assignedUser.name}
			alt={$t('profilePicture', { name: assignedUser.name })}
		/>
	{/if}

	<div class="to-do-task-content" class:assigned={assignedUser}>
		{#if stale}
			<span class="unchanged-since">{formattedStaleTaskDate}</span>
		{/if}

		{#if url}
			<a href={url} class="name" target="_blank" rel="noreferrer">{name}</a>
		{:else}
			<span class="name">{name}</span>
		{/if}

		<button
			type="button"
			{onclick}
			class="check-button"
			class:one-time={isOneTime}
			{disabled}
			title={$t('derivedList.complete')}
			aria-label={$t('derivedList.complete')}
		>
			<i class="far fa-square"></i>
			<i class="fas fa-check-square"></i>
			<i class="fas fa-trash-alt"></i>
		</button>
	</div>
</div>

<style lang="scss">
	.to-do-task {
		display: flex;
		justify-content: flex-start;
		margin-bottom: 7px;

		&:last-child {
			margin-bottom: 0;
		}

		&.active {
			.name {
				text-decoration: line-through;
				color: var(--faded-color);
			}

			.check-button {
				&:not(.one-time) {
					.fa-check-square {
						display: inline;
					}
					.fa-square {
						display: none;
					}
				}

				&.one-time .fa-trash-alt {
					color: var(--danger-color);
				}
			}
		}

		&-assignee-image {
			width: 34px;
			height: 34px;
			border-radius: 50%;
			margin: 6px 9px 0 0;
		}

		.check-button {
			min-width: 45px;
			background: transparent;
			border: none;
			outline: none;
			font-size: 27px;
			line-height: 45px;
			text-decoration: none;
			text-align: center;
			color: var(--primary-color);

			&:disabled {
				color: var(--faded-color);
			}

			&:not(:disabled):hover {
				color: var(--primary-color-dark);
			}

			.fa-check-square {
				display: none;
			}
		}

		&.high-priority .to-do-task-content .name {
			padding-left: 50px;
		}

		&-content {
			display: flex;
			justify-content: space-between;
			width: 100%;
			border-radius: 6px;

			.unchanged-since {
				padding: 9px 5px;
				line-height: 27px;
				white-space: nowrap;
				color: #dd7001;
			}

			.name {
				width: 100%;
				border-bottom: 1px solid #ddd;
				padding: 9px 5px;
				line-height: 27px;
				text-align: center;
			}
			a.name {
				color: var(--primary-color-dark);
			}
			&.assigned .name {
				padding: 9px 48px 9px 50px;
			}

			.fa-square,
			.one-time .fa-trash-alt {
				display: inline;
			}

			.fa-trash-alt,
			.one-time .fa-square {
				display: none;
			}

			.one-time {
				font-size: 23px;
			}
		}
	}
</style>
