<script lang="ts">
	import { onMount } from 'svelte';
	import { AuthService } from '../../../../../../Core/shared2/services/authService';
	import { LocalStorageKeys, LocalStorageUtil } from '$lib/utils/localStorageUtil';

	let containerHeight = 768;

	const localStorage = new LocalStorageUtil();

	onMount(async () => {
		if (window.innerHeight < containerHeight) {
			containerHeight = window.innerHeight;
		}

		const query = window.location.search;
		if (!query.includes('code=') || !query.includes('state=')) {
			throw new Error('Query parameters for redirect callback missing');
		}

		// Process the login state
		const authService = new AuthService();
		await authService.handleRedirectCallback();

		const immediateListId = localStorage.getObject<number | null>(LocalStorageKeys.ImmediateList);
		const location = immediateListId ? `/list/${immediateListId}` : '/lists';

		// Use replaceState to redirect the user away and remove the querystring parameters
		window.history.replaceState({}, document.title, location);
		window.location.href = location;
	});
</script>

<section class="container" style="height: {containerHeight}px">
	<div>
		<div class="loader-wrap">
			<div class="loader" />
			<i class="fas fa-lock" />
		</div>

		<div class="authorizing">Logging in</div>
	</div>
</section>

<style lang="scss">
	.container {
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.loader-wrap {
		position: relative;

		i {
			position: absolute;
			font-size: 38px;
			left: calc(50% - 17px);
			top: calc(50% - 18px);
			animation: swing 1.5s infinite;
		}
	}

	@keyframes swing {
		0% {
			transform: rotate(10deg);
		}
		50% {
			transform: rotate(-10deg);
		}
		100% {
			transform: rotate(10deg);
		}
	}

	.loader,
	.loader:after {
		border-radius: 50%;
		width: 9em;
		height: 9em;
	}
	.loader {
		margin: 35px auto;
		font-size: 12px;
		position: relative;
		text-indent: -9999em;
		border-top: 0.7em solid rgba(106, 104, 243, 0.3);
		border-right: 0.7em solid rgba(106, 104, 243, 0.3);
		border-bottom: 0.7em solid rgba(106, 104, 243, 0.3);
		border-left: 0.7em solid var(--regular-color);
		transform: translateZ(0);
		animation: load8 1.1s infinite linear;
	}
	@keyframes load8 {
		0% {
			transform: rotate(0deg);
		}
		100% {
			transform: rotate(360deg);
		}
	}

	.authorizing {
		font-size: 1.7rem;
		user-select: none;
	}
</style>
