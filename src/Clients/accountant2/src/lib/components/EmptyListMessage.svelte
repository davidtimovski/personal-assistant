<script lang="ts">
	import { t } from '$lib/localization/i18n';

	export let messageKey: string;

	let message = $t(messageKey);
</script>

<div class="empty-list-message">
	<span contenteditable="true" bind:innerHTML={message} />
</div>

<style lang="scss">
	.empty-list-message {
		display: block;
		padding: 45px 0;
		font-size: 1.3rem;
		line-height: 2rem;
		text-align: center;
		color: var(--faded-color);
		-webkit-user-select: none;
		-moz-user-select: none;
		-ms-user-select: none;
		user-select: none;
	}

	@media screen and (max-width: 550px) {
		.empty-list-message {
			padding: 35px 0;
		}
	}
</style>
