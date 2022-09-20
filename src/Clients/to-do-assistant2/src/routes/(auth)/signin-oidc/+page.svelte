<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';
	import pkg from 'oidc-client';
	const { UserManager, WebStorageStateStore } = pkg;
	import { AuthService } from '../../../../../shared2/services/authService';

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
				await new AuthService('to-do-assistant2', window).loginCallback();
				await goto('/');
			})
			.catch((e) => {
				console.error(e);
			});
	});
</script>

<section class="auth-wrap">
	<div class="loader-wrap">
		<div class="loader" />
		<i class="fas fa-lock" />
	</div>

	<div class="authorizing">AUTHORIZING</div>
</section>

<style lang="scss">
	.auth-wrap {
		margin-top: 50%;
		text-align: center;
		color: var(--primary-color);
	}

	.loader-wrap {
		position: relative;
	}

	.loader-wrap i {
		position: absolute;
		font-size: 38px;
		left: calc(50% - 17px);
		top: calc(50% - 18px);
		animation: swing 1.5s infinite;
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
		margin: 30px auto;
		font-size: 10px;
		position: relative;
		text-indent: -9999em;
		border-top: 1em solid rgba(106, 104, 243, 0.3);
		border-right: 1em solid rgba(106, 104, 243, 0.3);
		border-bottom: 1em solid rgba(106, 104, 243, 0.3);
		border-left: 1em solid var(--primary-color);
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
		font-size: 1.3rem;
		font-weight: bold;
		letter-spacing: 0.07rem;
		user-select: none;
	}
</style>
