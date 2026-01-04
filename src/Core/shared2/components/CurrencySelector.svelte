<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { AutocompleteResult } from 'autocompleter';
	import autocomplete from 'autocompleter';

	import { CurrenciesService } from '../services/currenciesService';
	import { CurrencySuggestion } from '../models/viewmodels/currencySuggestion';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';

	let { application }: { application: string } = $props();

	let currency: string | null = $state(null);
	let changing = $state(false);
	let autocompleteResult: AutocompleteResult;
	let selectCurrencyInput: HTMLInputElement | null;

	let localStorage: LocalStorageUtil;
	let currenciesService: CurrenciesService;

	function toggleChangeCurrency() {
		if (!changing) {
			(<HTMLInputElement>selectCurrencyInput).value = '';
			changing = true;
			(<HTMLInputElement>selectCurrencyInput).focus();
		} else {
			changing = false;
		}
	}

	onMount(() => {
		selectCurrencyInput = <HTMLInputElement>document.getElementById('select-currency-input');

		localStorage = new LocalStorageUtil();
		currenciesService = new CurrenciesService(application);

		currency = localStorage.get(LocalStorageKeys.Currency);
		const currencies = currenciesService.getCurrencies();

		autocompleteResult = autocomplete({
			input: selectCurrencyInput,
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

				localStorage.set('currency', currency);
			},
			className: 'currency-autocomplete-customizations'
		});
	});

	onDestroy(() => {
		currenciesService?.release();
	});
</script>

<div class="currency-selector-wrap">
	<label for="selected-currency">{$t('currency')}</label>
	<div class="currency-selector" class:changing>
		<input
			type="text"
			id="select-currency-input"
			class="select-currency-input"
			maxlength="3"
			placeholder={$t('preferences.change')}
			aria-label={$t('preferences.change')}
		/>
		<button type="button" id="selected-currency" onclick={toggleChangeCurrency} class="selected-currency">{currency}</button>
	</div>
</div>

<style lang="scss">
	.currency-selector-wrap {
		display: flex;
		justify-content: space-between;
		padding: 0 4px;
		line-height: 31px;

		.currency-selector {
			display: flex;
			line-height: 31px;

			.select-currency-input {
				opacity: 0;
				width: 70px;
				border-top-right-radius: 0;
				border-bottom-right-radius: 0;
				border-right: none;
				font-size: 1rem;
				line-height: 31px;
				transition: opacity var(--transition-quick);
			}

			&.changing .select-currency-input {
				opacity: 1;
			}

			.selected-currency {
				background: var(--primary-color);
				border: none;
				border-radius: var(--border-radius);
				outline: none;
				padding: 0 12px;
				font-size: inherit;
				line-height: 27px;
				cursor: pointer;
				color: #fff;
				transition:
					background var(--transition-quick),
					border-radius var(--transition-quick),
					box-shadow var(--transition-quick);
			}

			&.changing .selected-currency {
				background: var(--primary-color-dark);
				border-top-left-radius: 0;
				border-bottom-left-radius: 0;
				box-shadow: inset var(--box-shadow);
			}
		}
	}

	/* DESKTOP */
	@media screen and (min-width: 1200px) {
		.currency-selector-wrap,
		.currency-selector-wrap .currency-selector,
		.currency-selector-wrap .currency-selector .select-currency-input {
			line-height: 35px;
		}
		.currency-selector-wrap .currency-selector .select-currency-input {
			font-size: 1.2rem;
		}
	}
</style>
