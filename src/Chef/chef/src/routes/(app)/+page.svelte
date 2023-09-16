<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';

	import { authInfo } from '$lib/stores';

	let containerHeight = 768;

	onMount(() => {
		if (window.innerHeight < containerHeight) {
			containerHeight = window.innerHeight;
		}

		return authInfo.subscribe(async (x) => {
			if (!x) {
				return;
			}

			await goto('/recipes');
		});
	});
</script>

<section class="container" style="height: {containerHeight}px">
	<div>
		<div class="loader-wrap">
			<div class="loader" />
		</div>

		<div class="app-name">Chef</div>
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
		border-left: 0.7em solid var(--primary-color);
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

	.app-name {
		font-size: 1.7rem;
		user-select: none;
	}
</style>
