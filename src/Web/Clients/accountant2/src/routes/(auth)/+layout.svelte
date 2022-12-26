<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;
</script>

<main>
	<div />
	<div class="center">
		<slot />
	</div>
	<div />
</main>
