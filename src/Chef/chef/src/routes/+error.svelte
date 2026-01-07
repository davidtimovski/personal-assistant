<script lang="ts">
	import '../app.css';

	export const prerender = true;

	import { page } from '$app/stores';
	import { t } from '$lib/localization/i18n';
</script>

<main>
	<div></div>
	<div class="center">
		<section class="container">
			<div class="page-title-wrap">
				<div class="side inactive">
					<i class="fas fa-exclamation-circle"></i>
				</div>
				<div class="page-title">{$page.status}</div>
				<div class="side inactive">
					<i class="fas fa-exclamation-circle"></i>
				</div>
			</div>

			<div class="content-wrap">
				<div class="text-wrap not-found-message">
					{$t($page.status === 404 ? 'error.youSeemLost' : 'error.somethingWentWrong')}
				</div>

				<a href="/recipes" class="wide-button">{$t('error.takeMeHome')}</a>
			</div>
		</section>
	</div>
	<div></div>
</main>
