<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { ErrorLogger } from '../../../../../shared2/services/errorLogger';
	import { TooltipsService } from '../../../../../shared2/services/tooltipsService';
	import type { Tooltip } from '../../../../../shared2/models/tooltip';

	import { t } from '$lib/localization/i18n';

	let tooltips: Tooltip[];

	let tooltipsService: TooltipsService;

	async function dismiss(tooltip: Tooltip) {
		tooltip.isDismissed = true;
		await tooltipsService.toggleDismissed(tooltip.key, 'Accountant', true);
		tooltips = tooltips.slice(0);
	}

	async function retain(tooltip: Tooltip) {
		tooltip.isDismissed = false;
		await tooltipsService.toggleDismissed(tooltip.key, 'Accountant', false);
		tooltips = tooltips.slice(0);
	}

	onMount(async () => {
		tooltipsService = new TooltipsService(new ErrorLogger('Accountant', 'accountant2'));

		const accountantTooltips = await tooltipsService.getAll('Accountant');
		for (const tooltip of accountantTooltips) {
			tooltip.title = (<any>$t(`tooltips.${tooltip.key}`)).title;
			tooltip.answer = (<any>$t(`tooltips.${tooltip.key}`)).answer;
		}
		tooltips = accountantTooltips;
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-info-circle" />
		</div>
		<div class="page-title">{$t('help.help')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if !tooltips}
			<div class="double-circle-loading">
				<div class="double-bounce1" />
				<div class="double-bounce2" />
			</div>
		{:else}
			<div class="share-requests-wrap">
				{#each tooltips as tooltip}
					<div class="tooltip-item">
						<div class="tooltip-header">
							{#if tooltip.isDismissed}
								<button
									type="button"
									on:click={() => retain(tooltip)}
									title={$t('help.show')}
									aria-label={$t('help.show')}
								>
									<i class="fas fa-eye-slash" />
								</button>
							{:else}
								<button
									type="button"
									on:click={() => dismiss(tooltip)}
									title={$t('help.hide')}
									aria-label={$t('help.hide')}
								>
									<i class="fas fa-eye" />
								</button>
							{/if}

							<span class="tooltip-title">{tooltip.title}</span>
							<i class="side hidden">
								<i class="fas fa-eye" />
							</i>
						</div>
						<hr />
						<div class="tooltip-answer">{tooltip.answer}</div>
					</div>
				{/each}
			</div>
		{/if}
	</div>
</section>

<style lang="scss">
	.tooltip-item {
		border: 1px solid #ddd;
		border-radius: 8px;
		padding: 5px;
		margin-bottom: 10px;

		hr {
			margin: 3px 10%;
		}
	}

	.tooltip-header {
		display: flex;
		justify-content: space-between;

		button {
			width: 54px;
			background: transparent;
			border: none;
			outline: none;
			padding: 3px 0 1px;
			font-size: 25px;
			text-align: center;
			line-height: 40px;
			text-decoration: none;
			color: var(--primary-color);
			transition: color var(--transition);

			&:hover {
				color: var(--primary-color-dark);
			}
		}
	}

	.fa-eye-slash {
		color: var(--faded-color);
	}

	.tooltip-title {
		display: inline-flex;
		justify-content: center;
		align-items: center;
		padding: 10px 5px;
		font-size: 1.1rem;
		line-height: 1.4rem;
	}

	.tooltip-answer {
		padding: 8px 10px;
		font-size: 1rem;
		line-height: 1.3rem;
		text-align: center;
	}
</style>
