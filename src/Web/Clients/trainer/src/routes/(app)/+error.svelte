<script lang="ts">
	import { page } from '$app/stores';
	import { t } from '$lib/localization/i18n';
</script>

<section class="container">
	{#if $page.status === 404}
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
		<h1>{$page.error?.message}</h1>
	{/if}
</section>
