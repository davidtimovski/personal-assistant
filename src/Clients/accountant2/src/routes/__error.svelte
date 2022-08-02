<script context="module">
	// @ts-ignore
	export function load({ error, status }) {
		return {
			props: {
				title: `${status}: ${error.message}`
			}
		};
	}
</script>

<script>
	export /**
	 * @type {any}
	 */
	let title;
</script>

<h1>{title}</h1>
