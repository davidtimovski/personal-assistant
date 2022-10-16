import { derived } from 'svelte/store';
import { locale } from '$lib/stores';
import translations from '$lib/localization/translations';

export const locales = Object.keys(translations);

function translate(locale: string, key: string, vars: any) {
	const lookup = (<any>translations)[locale];
	let text: string;

	const parts = key.split('.');
	if (parts.length === 1) {
		text = lookup[key];
	} else {
		text = lookup[parts[0]][parts[1]];
	}

	if (!text) throw new Error(`no translation found for ${locale}.${key}`);

	// Replace any passed in variables in the translation string.
	Object.keys(vars).map((k: any) => {
		const regex = new RegExp(`{{${k}}}`, 'g');
		text = text.replace(regex, vars[k]);
	});

	return text;
}

export const t = derived(
	locale,
	($locale) =>
		(key: string, vars = {}) =>
			translate($locale, key, vars)
);
