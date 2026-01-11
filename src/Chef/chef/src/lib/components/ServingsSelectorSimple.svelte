<script lang="ts">
	import { t } from '$lib/localization/i18n';

	let { servings = $bindable<number>() }: { servings: number } = $props();

	function decrementServings() {
		if (servings > 1) {
			servings--;
		}
	}

	function incrementServings() {
		if (servings < 50) {
			servings++;
		}
	}

	let servingsLabel = $derived(servings > 1 ? $t('recipe.servings', { servings: servings }) : $t('recipe.oneServing'));
</script>

<div class="servings-selector">
	<a onclick={decrementServings} class="servings-button" role="button" title={$t('decreaseServings')} aria-label={$t('decreaseServings')}>
		<i class="fas fa-minus"></i>
	</a>
	<span class="servings">{servingsLabel}</span>
	<a onclick={incrementServings} class="servings-button" role="button" title={$t('increaseServings')} aria-label={$t('increaseServings')}>
		<i class="fas fa-plus"></i>
	</a>
</div>
