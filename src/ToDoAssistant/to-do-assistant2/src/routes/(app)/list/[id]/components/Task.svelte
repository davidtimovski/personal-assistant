<script lang="ts">
	import { t } from '$lib/localization/i18n';
	import type { Assignee } from '$lib/models/viewmodels/assignee';

	let {
		active,
		disabled,
		highPriority,
		highlighted,
		assignedUser = null,
		id,
		name,
		url,
		completed,
		isOneTime,
		onclick
	}: {
		active: boolean;
		disabled: boolean;
		highPriority: boolean;
		highlighted: boolean;
		assignedUser?: Assignee | null;
		id: number;
		name: string;
		url: string | null;
		completed: boolean;
		isOneTime: boolean;
		onclick: any;
	} = $props();
</script>

<div class="to-do-task" class:completed class:active class:high-priority={highPriority}>
	<div class="to-do-task-content" class:highlighted class:assigned={assignedUser}>
		<a href="/editTask/{id}" class="edit-button" title={$t('list.edit')} aria-label={$t('list.edit')}>
			<i class="fas fa-pencil-alt"></i>
		</a>

		{#if assignedUser}
			<img
				src={assignedUser.imageUri}
				class="to-do-task-assignee-image"
				title={assignedUser.name}
				alt={$t('profilePicture', { name: assignedUser.name })}
			/>
		{/if}

		{#if url}
			<a href={url} class="name" target="_blank" rel="noreferrer">{name}</a>
		{:else}
			<span class="name">{name}</span>
		{/if}

		<button
			type="button"
			{onclick}
			class:check-button={!completed}
			class:uncheck-button={completed}
			class:one-time={isOneTime}
			{disabled}
			title={$t(completed ? 'list.uncomplete' : 'list.complete')}
			aria-label={$t(completed ? 'list.uncomplete' : 'list.complete')}
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

			.uncheck-button {
				.fa-square {
					display: inline;
				}
				.fa-check-square {
					display: none;
				}
			}
		}

		&.completed {
			opacity: 0.6;

			&:hover {
				opacity: 1;
			}
		}

		&.high-priority .name {
			font-weight: bold;
			color: var(--danger-color-dark);
		}

		&-assignee-image {
			width: 34px;
			height: 34px;
			border-radius: 50%;
			margin: 6px 9px 0 4px;
		}

		.fa-square,
		.fa-check-square,
		.one-time .fa-trash-alt {
			display: inline;
		}

		.fa-trash-alt,
		.one-time .fa-square,
		.one-time .fa-check-square {
			display: none;
		}

		.one-time {
			font-size: 23px;
		}

		.edit-button {
			min-width: 60px;
			font-size: 23px;
			line-height: 45px;
			text-decoration: none;
			text-align: center;
			color: var(--primary-color);

			&:hover {
				color: var(--primary-color-dark);
			}
		}

		.check-button,
		.uncheck-button {
			min-width: 45px;
			background: transparent;
			border: none;
			outline: none;
			font-size: 27px;
			line-height: 45px;
			text-align: center;
			color: var(--primary-color);

			&:disabled {
				color: var(--faded-color);
			}

			&:not(:disabled):hover {
				color: var(--primary-color-dark);
			}
		}

		.check-button .fa-check-square,
		.uncheck-button .fa-square {
			display: none;
		}

		&-content {
			display: flex;
			justify-content: space-between;
			width: 100%;
			border-radius: 6px;

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
				padding: 9px 52px 9px 5px;
			}
		}
	}
</style>
