<script lang="ts">
	import { t } from '$lib/localization/i18n';

	export let servings: number;

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

	$: servingsLabel = servings > 1 ? $t('recipe.servings', { servings: servings }) : $t('recipe.oneServing');
</script>

<div class="servings-selector">
	<a
		on:click={decrementServings}
		class="servings-button"
		role="button"
		title={$t('decreaseServings')}
		aria-label={$t('decreaseServings')}
	>
		<i class="fas fa-minus" />
	</a>
	<span class="servings">{servingsLabel}</span>
	<a
		on:click={incrementServings}
		class="servings-button"
		role="button"
		title={$t('increaseServings')}
		aria-label={$t('increaseServings')}
	>
		<i class="fas fa-plus" />
	</a>
</div>
