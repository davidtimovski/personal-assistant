<script lang="ts">
	import pkg from 'oidc-client';
	const { UserManager, WebStorageStateStore } = pkg;
	import { onMount } from 'svelte/internal';
	import { AuthService } from '../../../shared2/services/authService';

	onMount(() => {
		new UserManager({
			response_mode: 'query',
			userStore: new WebStorageStateStore({
				prefix: 'oidc',
				store: window.localStorage
			})
		})
			.signinRedirectCallback()
			.then(async () => {
				await new AuthService(window).loginCallback();
				window.location.href = '/';
			})
			.catch((e) => {
				console.error(e);
			});
	});
</script>
