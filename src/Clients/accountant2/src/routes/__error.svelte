<script context="module">
	// @ts-ignore
	export function load({ error, status }) {
		return {
			props: {
				errorMessage: error.message,
				status
			}
		};
	}
</script>

<script lang="ts">
	import { t } from '$lib/localization/i18n';

	export let errorMessage: string;
	export let status: number;
</script>

<section class="container">
	{#if status === 404}
		<div class="page-title-wrap">
			<div class="side inactive">
				<i class="fas fa-exclamation-circle" />
			</div>
			<div class="page-title">404</div>
			<div class="side inactive">
				<i class="fas fa-exclamation-circle" />
			</div>
		</div>

		<div class="content-wrap">
			<div class="text-wrap not-found-message">{$t('notFound.youSeemLost')}</div>

			<a href="/" class="wide-button">{$t('notFound.takeMeHome')}</a>
		</div>
	{:else}
		<h1>{errorMessage}</h1>
	{/if}
</section>
