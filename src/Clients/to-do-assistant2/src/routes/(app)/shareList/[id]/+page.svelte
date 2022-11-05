<script lang="ts">
	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	import { ValidationResult, ValidationUtil } from '../../../../../../shared2/utils/validationUtils';
	import DoubleRadioBool from '../../../../../../shared2/components/DoubleRadioBool.svelte';
	import Tooltip from '../../../../../../shared2/components/Tooltip.svelte';

	import { t } from '$lib/localization/i18n';
	import { user, alertState } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { Share } from '$lib/models/entities';
	import { ShareUserAndPermission } from '$lib/models/viewmodels/shareUserAndPermission';
	import { SharingState } from '$lib/models/viewmodels/sharingState';
	import type { CanShareList } from '$lib/models/viewmodels/canShareList';

	export let data: PageData;

	let name = '';
	let sharingState: SharingState;
	let ownerEmail = '';
	let ownerName = '';
	let ownerImageUri = '';
	let userShare: Share;
	let shares: Array<Share>;
	let originalShares: Share[];
	let emailIsInvalid = false;
	let emailInput: HTMLInputElement;
	let canSave = false;
	let newShares = new Array<ShareUserAndPermission>();
	let editedShares = new Array<ShareUserAndPermission>();
	let removedShares = new Array<ShareUserAndPermission>();
	let saveButtonIsLoading = false;
	let membersLabel = '';
	const unsubscriptions: Unsubscriber[] = [];

	// Selected share
	let selectedShareUserId = 0;
	let selectedShareEmail = '';
	let selectedShareName = '';
	let selectedShareImageUri = '';
	let selectedShareIsAdmin = false;

	unsubscriptions.push(
		alertState.subscribe((value) => {
			if (value.hidden) {
				emailIsInvalid = false;
			}
		})
	);

	let listsService: ListsService;

	$: emailPlaceholderText = $t(selectedShareIsAdmin ? 'shareList.newAdminEmail' : 'shareList.newMemberEmail');

	$: canSave = newShares.length + editedShares.length + removedShares.length > 0;

	function resetSelectedShare() {
		selectedShareUserId = 0;
		selectedShareEmail = '';
		selectedShareName = '';
		selectedShareImageUri = '';
		selectedShareIsAdmin = false;
		membersLabel = $t('shareList.members');
	}

	function validate(): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(selectedShareEmail)) {
			result.fail('email');
		}

		if (selectedShareEmail.trim().toLowerCase() === $user.email) {
			result.fail('email');
		}

		return result;
	}

	async function addShare() {
		alertState.update((x) => {
			x.hide();
			return x;
		});

		const email = selectedShareEmail.trim();

		const duplicateEmails = shares.filter((share) => {
			if (share.email === email) {
				return share;
			}
		});
		if (duplicateEmails.length > 0 || ownerEmail === email) {
			emailIsInvalid = true;
			return;
		}

		const result = validate();

		if (result.valid) {
			emailIsInvalid = false;

			const canShareVM: CanShareList = await listsService.canShareListWithUser(email);

			if (canShareVM.userId === 0) {
				emailIsInvalid = true;

				alertState.update((x) => {
					x.showError('shareList.userDoesntExist');
					return x;
				});
			} else {
				if (!canShareVM.canShare) {
					emailIsInvalid = true;
					alertState.update((x) => {
						x.showError('shareList.cannotShareWithUser');
						return x;
					});
				} else {
					selectedShareUserId = canShareVM.userId;
					selectedShareImageUri = canShareVM.imageUri;

					const selectedShare = new Share(
						selectedShareUserId,
						selectedShareEmail,
						selectedShareName,
						selectedShareImageUri,
						selectedShareIsAdmin,
						false,
						null
					);
					shares.push(selectedShare);

					if (shareExistedPreviously(selectedShareUserId)) {
						removedShares.splice(removedShares.indexOf(selectedShare), 1);
						editedShares.push(selectedShare);
					} else {
						newShares.push(selectedShare);
					}

					resetSelectedShare();
				}
			}
		} else {
			emailIsInvalid = true;
		}
	}

	async function saveShare() {
		if (
			!newShares.find((x) => x.userId === selectedShareUserId) &&
			!editedShares.find((x) => x.userId === selectedShareUserId)
		) {
			editedShares = editedShares.concat(
				new Share(
					selectedShareUserId,
					selectedShareEmail.trim(),
					selectedShareName,
					selectedShareImageUri,
					selectedShareIsAdmin,
					false,
					null
				)
			);
		}

		const share = <Share>shares.find((x) => x.userId === selectedShareUserId);
		share.isAdmin = selectedShareIsAdmin;
		shares = [...shares];

		resetSelectedShare();
	}

	function submit() {
		if (selectedShareUserId === 0) {
			addShare();
		}
	}

	function removeShare(share: Share) {
		shares = shares.filter((x) => x.userId !== share.userId);
		newShares = newShares.filter((x) => x.userId !== share.userId);
		editedShares = editedShares.filter((x) => x.userId !== share.userId);

		if (shareExistedPreviously(share.userId)) {
			removedShares.push(new ShareUserAndPermission(share.userId, false));
		}

		if (share.email === selectedShareEmail) {
			resetSelectedShare();
		}
	}

	function select(share: Share) {
		emailIsInvalid = false;

		if (selectedShareUserId === share.userId) {
			resetSelectedShare();
		} else {
			selectedShareUserId = share.userId;
			selectedShareEmail = share.email;
			selectedShareName = share.name;
			selectedShareImageUri = share.imageUri;
			selectedShareIsAdmin = share.isAdmin;

			membersLabel = $t('shareList.editing');
		}
	}

	async function save() {
		saveButtonIsLoading = true;
		emailIsInvalid = false;

		await listsService.share(data.id, newShares, editedShares, removedShares);

		if (editedShares.length + removedShares.length > 0) {
			alertState.update((x) => {
				x.showSuccess('shareList.sharingDetailsSaved');
				return x;
			});
		} else if (newShares.length === 1) {
			alertState.update((x) => {
				x.showSuccess('shareList.shareRequestSent');
				return x;
			});
		} else if (newShares.length > 1) {
			alertState.update((x) => {
				x.showSuccess('shareList.shareRequestsSent');
				return x;
			});
		}

		await goto('/?edited=' + data.id);
	}

	function shareExistedPreviously(userId: number): boolean {
		return !!originalShares.find((x) => x.userId === userId);
	}

	onMount(async () => {
		membersLabel = $t('shareList.members');

		listsService = new ListsService();

		const list = await listsService.getWithShares(data.id);
		if (!list) {
			throw new Error('List not found');
		}

		name = list.name;
		sharingState = list.sharingState;
		ownerEmail = list.ownerEmail;
		ownerName = list.ownerName;
		ownerImageUri = list.ownerImageUri;
		userShare = list.userShare;
		shares = list.shares;

		originalShares = list.shares.slice();

		if (sharingState === SharingState.NotShared) {
			emailInput.focus();
		}
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		listsService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive small">
			<i class="fas fa-handshake" />
		</div>
		<div class="page-title">
			<span>{$t('shareList.share')}</span>&nbsp;<span class="colored-text">{name}</span>
		</div>
		<a href="/list/{data.id}" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		{#if sharingState !== 4}
			<form on:submit|preventDefault={submit}>
				<div class="form-control">
					<div class="add-input-wrap">
						<input
							type="email"
							bind:value={selectedShareEmail}
							bind:this={emailInput}
							disabled={selectedShareUserId !== 0}
							maxlength="256"
							class:invalid={emailIsInvalid}
							placeholder={emailPlaceholderText}
							aria-label={emailPlaceholderText}
							required
						/>

						{#if selectedShareUserId !== 0}
							<button type="button" on:click={saveShare} title={$t('save')} aria-label={$t('save')}>
								<i class="fas fa-save" />
							</button>
						{:else}
							<button
								type="button"
								on:click={addShare}
								title={$t('shareList.addMember')}
								aria-label={$t('shareList.addMember')}
							>
								<i class="fas fa-plus-circle" />
							</button>
						{/if}
					</div>
				</div>
				<div class="form-control">
					<DoubleRadioBool
						name="permissionsToggle"
						leftLabelKey="shareList.member"
						rightLabelKey="shareList.admin"
						bind:value={selectedShareIsAdmin}
					/>

					<Tooltip key="memberVsAdmin" application="ToDoAssistant" />
				</div>
			</form>
		{/if}

		<div class="labeled-separator">
			<div class="labeled-separator-text">{membersLabel}</div>
			<hr />
		</div>

		<div class="share-with">
			<img class="share-image" src={ownerImageUri} title={ownerName} alt={$t('profilePicture', { name: ownerName })} />
			<div class="share-content not-editable">
				<div class="icon" title={$t('shareList.owner')} aria-label={$t('shareList.owner')}>
					<i class="fas fa-crown" />
				</div>
				<span class="name">{ownerEmail}</span>
			</div>
		</div>

		{#if userShare}
			<div class="share-with">
				<img
					class="share-image"
					src={userShare.imageUri}
					title={userShare.name}
					alt={$t('profilePicture', { name: userShare.name })}
				/>
				<div class="share-content not-editable">
					{#if userShare.isAdmin}
						<div class="icon" title={$t('shareList.admin')} aria-label={$t('shareList.admin')}>
							<i class="fas fa-user-tie" />
						</div>
					{:else}
						<div class="icon" title={$t('shareList.member')} aria-label={$t('shareList.member')}>
							<i class="fas fa-user" />
						</div>
					{/if}

					<span class="name">{userShare.email}</span>
				</div>
			</div>
		{/if}

		{#if shares}
			{#each shares as share}
				<div class="share-with">
					<img
						class="share-image"
						src={share.imageUri}
						title={share.name}
						alt={$t('profilePicture', { name: share.name })}
					/>
					<div class="share-content" class:selected={share.email === selectedShareEmail}>
						{#if share.userId && !share.createdDate && share.userId !== 0 && share.email !== selectedShareEmail}
							<div class="icon" title={$t('shareList.newlyAddedMember')} aria-label={$t('shareList.newlyAddedMember')}>
								<i class="fas fa-user-plus" />
							</div>
						{/if}

						{#if share.isAccepted === null && !!share.createdDate && share.email !== selectedShareEmail}
							<div class="icon" title={$t('shareList.pendingAccept')} aria-label={$t('shareList.pendingAccept')}>
								<i class="fas fa-user-clock" />
							</div>
						{/if}

						{#if share.isAccepted && !share.isAdmin && share.email !== selectedShareEmail}
							<div class="icon" title={$t('shareList.member')} aria-label={$t('shareList.member')}>
								<i class="fas fa-user" />
							</div>
						{/if}

						{#if share.isAccepted && share.isAdmin && share.email !== selectedShareEmail}
							<div class="icon" title={$t('shareList.admin')} aria-label={$t('shareList.admin')}>
								<i class="fas fa-user-tie" />
							</div>
						{/if}

						{#if share.email === selectedShareEmail}
							<div class="share-content-button" title={$t('shareList.editing')} aria-label={$t('shareList.editing')}>
								<i class="fas fa-user-edit" />
							</div>
						{/if}

						<button type="button" on:click={() => select(share)} class="name">{share.email}</button>

						<button
							type="button"
							on:click={() => removeShare(share)}
							class="share-content-button remove-button"
							title={$t('shareList.remove')}
							aria-label={$t('shareList.remove')}
						>
							<i class="fas fa-times-circle" />
						</button>
					</div>
				</div>
			{/each}
		{/if}

		<hr />

		<div class="save-delete-wrap">
			<button type="button" on:click={save} class="button primary-button" disabled={!canSave || saveButtonIsLoading}>
				<span class="button-loader" class:loading={saveButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('save')}</span>
			</button>
			<a href="/list/{data.id}" class="button secondary-button">{$t('cancel')}</a>
		</div>
	</div>
</section>

<style lang="scss">
	.add-input-wrap {
		position: relative;

		input {
			width: calc(100% - 58px);
			padding-right: 46px;
			line-height: 45px;
		}

		button {
			position: absolute;
			top: 0;
			right: 0;
			background: none;
			border: none;
			outline: none;
			padding: 0 10px;
			font-size: 23px;
			line-height: 45px;
			color: var(--primary-color);

			&:hover {
				color: var(--primary-color-dark);
			}

			&:disabled {
				color: var(--faded-color);
				cursor: default;
			}
		}
	}

	.share-with {
		display: flex;
		justify-content: flex-start;
		margin-bottom: 10px;

		.share-image {
			width: 34px;
			height: 34px;
			border-radius: 50%;
			margin: 6px 12px 0 0;
		}

		.share-content {
			display: flex;
			justify-content: space-between;
			width: calc(100% - 48px);
			background: #fff;
			border: 1px solid #ddd;
			border-radius: 8px;

			.icon {
				display: flex;
				justify-content: center;
				align-items: center;
				min-width: 50px;
				height: 43px;
				font-size: 23px;
				color: var(--faded-color);
			}

			.name {
				width: 100%;
				background: transparent;
				border: none;
				outline: none;
				padding: 8px 10px 8px 2px;
				line-height: 27px;
				text-align: center;
				word-wrap: anywhere;
			}

			&-button {
				display: flex;
				justify-content: center;
				align-items: center;
				min-width: 45px;
				height: 43px;
				background: transparent;
				border: none;
				outline: none;
				font-size: 23px;
				color: var(--primary-color);

				&:hover {
					color: var(--primary-color-dark);
				}
			}

			&.not-editable {
				background: #e9f4ff;
				border-color: transparent;
				padding-right: 45px;

				.name {
					cursor: default;
					transition: none;
				}
			}

			&:not(.not-editable):not(.selected) .name:hover {
				color: var(--primary-color-dark);
			}

			&.selected {
				background: var(--primary-color);
				color: #fff;

				.share-content-button,
				.name {
					color: #fff;
				}

				.remove-button {
					visibility: hidden;
				}
			}
		}
	}

	/* Workaround for sticky :hover on mobile devices */
	.touch-device {
		.share-with:not(.owner) .name:hover,
		.add-input-wrap button:hover {
			color: var(--primary-color);
		}
	}

	@media screen and (min-width: 1200px) {
		.add-input-wrap button {
			font-size: 25px;
			line-height: 45px;
		}
	}
</style>
