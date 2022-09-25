<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { slide } from 'svelte/transition';

	import { t } from '$lib/localization/i18n';

	import type { Tooltip } from '../../../../shared2/models/tooltip';
	import { TooltipsService } from '../../../../shared2/services/tooltipsService';
	import { ErrorLogger } from '../../../../shared2/services/errorLogger';

	export let key: string;

	let tooltip: Tooltip | null = null;
	let isVisible = false;
	let isOpen = false;
	let isDismissed = false;
	let questionSpan: HTMLSpanElement;

	let tooltipsService: TooltipsService;

	function toggleOpen() {
		isOpen = !isOpen;
		questionSpan.classList.remove('glow');
	}

	async function dismiss() {
		if (!tooltip) {
			return;
		}

		isDismissed = true;
		await tooltipsService.toggleDismissed(tooltip.key, 'To', true);
	}

	onMount(async () => {
		tooltipsService = new TooltipsService(new ErrorLogger('ToDoAssistant'));

		tooltip = await tooltipsService.getByKey(key, 'ToDoAssistant');
		if (!tooltip.isDismissed) {
			tooltip.question = (<any>$t(`tooltips.${key}`)).question;
			tooltip.answer = (<any>$t(`tooltips.${key}`)).answer;
			isVisible = true;
		}
	});
</script>

<div class="tooltip" class:visible={isVisible}>
	{#if tooltip}
		<div on:click={toggleOpen} class="question-wrap">
			<span bind:this={questionSpan} class="question glow">{tooltip.question}</span>
		</div>
		{#if isOpen}
			<div in:slide class="answer-wrap">
				<div class="answer">
					<i class="fas fa-info-circle" class:faded={isDismissed} />
					<span>{tooltip.answer}</span>
					<div class="dismiss-wrap">
						<button type="button" on:click={dismiss} disabled={isDismissed}>{$t('tooltips.okayUnderstood')}</button>
					</div>
				</div>
			</div>
		{/if}

		{#if isDismissed}
			<div in:slide class="dismissed-wrap">
				<div class="dismissed">
					<i class="fas fa-arrow-alt-circle-up" />
					<span>{$t('tooltips.tooltipWillNoLongerShow')}</span>
				</div>
			</div>
		{/if}
	{/if}
</div>

<style lang="scss">
	.tooltip {
		display: none;
		font-size: 1rem;

		&.visible {
			display: block;
		}

		.question-wrap {
			margin: 10px 0;
			text-align: right;
			color: var(--primary-color);
		}

		.question {
			text-decoration: underline;
			cursor: pointer;

			&.glow {
				animation: glow 1s ease-in-out infinite alternate;
			}
		}

		.answer-wrap {
			background: #fff;
			border: 1px solid #ddd;
			border-radius: 8px;
			margin-bottom: 10px;
		}

		.answer,
		.dismissed {
			padding: 10px 15px;
			line-height: 1.5rem;
		}

		.fa-info-circle,
		.fa-arrow-alt-circle-up {
			margin-right: 4px;
			color: var(--primary-color);
		}

		.fa-info-circle.faded {
			color: var(--faded-color);
		}

		.answer .dismiss-wrap {
			margin: 10px 0 3px;
			text-align: right;

			button {
				background: linear-gradient(225deg, #00a6ed, #7a46f3);
				background-size: 400% 400%;
				border: none;
				border-radius: var(--border-radius);
				outline: none;
				box-shadow: var(--box-shadow);
				padding: 0 12px;
				font-size: 1rem;
				line-height: 31px;
				color: #fff;

				animation: AnimateGradiant 8s ease infinite;
				transition: border-radius var(--transition);

				&:disabled {
					opacity: 0.6;
				}
			}
		}

		.dismissed-wrap {
			background: #fff;
			border: 1px solid #ddd;
			border-radius: 8px;
			margin: 10px 0;
		}
	}

	/* DESKTOP */
	@media screen and (min-width: 1200px) {
		.tooltip {
			font-size: 1.2rem;
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device .tooltip .question:hover {
		color: var(--primary-color);
	}
</style>
