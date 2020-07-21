const passwordShowButtons = document.querySelectorAll('.password-show-button');

for (let button of passwordShowButtons) {
    const forField = button.dataset.for;
    const passwordInput = document.getElementById(forField);

    button.addEventListener('click', () => {
        const type = passwordInput.getAttribute('type');

        if (type === 'password') {
            passwordInput.setAttribute('type', 'text');
            button.innerText = window.hideLabel;
        } else {
            passwordInput.setAttribute('type', 'password');
            button.innerText = window.showLabel;
        }
    });
}