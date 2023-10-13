<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { AutocompleteResult } from 'autocompleter';
	import autocomplete from 'autocompleter';

	import { CurrenciesService } from '../../../../../Core/shared2/services/currenciesService';
	import { CurrencySuggestion } from '../../../../../Core/shared2/models/viewmodels/currencySuggestion';

	import { t } from '$lib/localization/i18n';

	export let amount: number | null;
	export let currency: string | null;
	export let invalid = false;
	export let inputId: string = 'amount';
	export let focusOnInit = false;
	export let disabled = false;

	let changing = false;
	let autocompleteResult: AutocompleteResult;
	let amountInput: HTMLInputElement | null;
	let selectCurrencyInput: HTMLInputElement | null;
	let min = 0.01;
	let max = 1500000;
	let step = 'any';

	let currenciesService: CurrenciesService;

	function toggleChangeCurrency() {
		if (!changing) {
			(<HTMLInputElement>selectCurrencyInput).value = '';
			changing = true;

			// Focus after input element appears
			window.setTimeout(() => {
				(<HTMLInputElement>selectCurrencyInput).focus();
			}, 0);
		} else {
			changing = false;
		}
	}

	onMount(() => {
		if (currency === 'MKD') {
			min = 1;
			max = 100000000;
			step = '1';
		}

		currenciesService = new CurrenciesService('Accountant');
		const currencies = currenciesService.getCurrencies();

		autocompleteResult = autocomplete({
			input: <HTMLInputElement>selectCurrencyInput,
			minLength: 1,
			fetch: (text: string, update: (items: CurrencySuggestion[]) => void) => {
				const suggestions = currencies
					.map((currency: string) => {
						return new CurrencySuggestion(currency, currency);
					})
					.filter((i) => i.name.toUpperCase().startsWith(text.toUpperCase()));
				update(suggestions);
			},
			onSelect: (suggestion: CurrencySuggestion) => {
				changing = false;
				currency = suggestion.name;

				if (!amount) {
					window.setTimeout(() => {
						amountInput?.focus();
					}, 0);
				}
			},
			className: 'currency-autocomplete-customizations'
		});

		if (focusOnInit) {
			(<HTMLInputElement>amountInput).focus();
		}
	});

	onDestroy(() => {
		currenciesService?.release();
	});
</script>

<div class="amount-input-wrap" class:changing>
	<input type="number" id={inputId} bind:this={amountInput} bind:value={amount} {disabled} {min} {max} {step} class:invalid required />

	<input
		type="text"
		bind:this={selectCurrencyInput}
		class="select-currency-input"
		maxlength="3"
		placeholder={$t('currency')}
		aria-label={$t('currency')}
	/>
	<button
		type="button"
		on:click={toggleChangeCurrency}
		class="selected-currency-button"
		title={$t('changeCurrency')}
		aria-label={$t('changeCurrency')}
	>
		{currency}
	</button>
</div>

<style lang="scss">
	.amount-input-wrap {
		display: inline-flex;
		line-height: 37px;

		input[type='number'] {
			width: 80px;
			border-top-right-radius: 0;
			border-bottom-right-radius: 0;
			border-right: none;
			text-align: center;

			&:disabled {
				opacity: 0.6;
			}
		}

		.select-currency-input {
			display: none;
			width: 80px;
			border-top-right-radius: 0;
			border-bottom-right-radius: 0;
			border-right: none;
			font-size: 1rem;
			line-height: 37px;
		}

		.selected-currency-button {
			background: #ddd;
			border: none;
			border-radius: var(--border-radius);
			border-top-left-radius: 0;
			border-bottom-left-radius: 0;
			border-left: none;
			outline: none;
			padding: 0 12px;
			font-size: inherit;
			line-height: 27px;
			color: inherit;
			transition: background var(--transition-quick), box-shadow var(--transition-quick), color var(--transition-quick);
		}

		&.changing {
			input[type='number'] {
				display: none;
			}

			.select-currency-input {
				display: inline-block;
			}

			.selected-currency-button {
				background: var(--primary-color);
				box-shadow: inset var(--box-shadow);
				color: #fff;
			}
		}
	}

	@media screen and (min-width: 1200px) {
		.amount-input-wrap input[type='number'] {
			width: 100px;
		}

		.amount-input-wrap .select-currency-input {
			font-size: 1.2rem;
			line-height: 45px;
		}
	}
</style>
