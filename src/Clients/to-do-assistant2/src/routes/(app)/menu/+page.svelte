<script lang="ts">
	import { onMount } from 'svelte/internal';

	import { AuthService } from '../../../../../shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import Variables from '$lib/variables';

	const personalAssistantUrl = Variables.urls.authority;
	let version = '--';

	async function logOut() {
		window.localStorage.removeItem('profileImageUriLastLoad');
		const authService = new AuthService('to-do-assistant2', window);
		await authService.logout();
	}

	onMount(async () => {
		const cacheNames = await caches.keys();
		if (cacheNames.length > 0) {
			version = cacheNames.sort().reverse()[0];
		}
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-bars" />
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<a href="/help" class="wide-button">{$t('menu.help')}</a>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<a href={personalAssistantUrl} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
			<button type="button" on:click={logOut} class="wide-button">{$t('menu.logOut')}</button>
		</div>

		<hr />

		<div class="version"><span>{$t('menu.version')}</span> {version}</div>
	</div>
</section>
