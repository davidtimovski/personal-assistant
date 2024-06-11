<script lang="ts">
	import { t } from '$lib/localization/i18n';

	const pluralizableUnits = ['cup', 'tbsp', 'tsp'];

	export let amount: number;
	export let unit: string;

	function decimalToFractionString(number: number): string {
		if (Number.isInteger(number)) {
			return new Intl.NumberFormat().format(number).toString();
		}

		const integerPart = Math.floor(number);
		const decimalPart = parseFloat((number - Math.floor(number)).toFixed(2));
		let fraction: string | undefined;

		switch (decimalPart) {
			case 0.5:
				fraction = '1/2';
				break;
			case 0.25:
				fraction = '1/4';
				break;
			case 0.75:
				fraction = '3/4';
				break;
			case 0.33:
				fraction = '1/3';
				break;
			case 0.66:
				fraction = '2/3';
				break;
			case 0.2:
				fraction = '1/5';
				break;
			case 0.4:
				fraction = '2/5';
				break;
			case 0.6:
				fraction = '3/5';
				break;
			case 0.8:
				fraction = '4/5';
				break;
		}

		if (fraction) {
			if (integerPart >= 1) {
				return integerPart + ' ' + fraction;
			}

			return fraction;
		}

		return new Intl.NumberFormat().format(number).toString();
	}

	$: amountLabel = () => {
		if (unit === 'pinch') {
			return 'pinch';
		}

		if (!amount) {
			return '';
		}

		const amountText = decimalToFractionString(amount);
		if (!unit) {
			return amount;
		}

		const unitText = amount > 1 && pluralizableUnits.includes(unit) ? $t(`recipe.${unit}Plural`) : $t(unit);

		return amountText + ' ' + unitText;
	};
</script>

<div class="ingredient-amount">
	{amountLabel()}
</div>

<style lang="scss">
	.ingredient-amount {
		white-space: nowrap;
	}
</style>
