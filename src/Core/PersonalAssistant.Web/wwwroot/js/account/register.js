grecaptcha.ready(async () => {
    try {
        const token = await grecaptcha.execute('6LfpNqYUAAAAAPlCVjWxJQEKQKmQMKqXDBFrtjAX', { action: 'register' });

        const response = await fetch('/account/verify-recaptcha', {
            method: 'post',
            body: JSON.stringify({ token: token })
        });

        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }

        const score = await response.text();

        document.getElementById('GoogleReCaptchaScore').value = score;
        document.getElementById('register-button').removeAttribute('disabled');
    } catch (error) {
        console.error(error.message);
        alert(error.message);
    }
});

(async function () {
    const response = await fetch('https://api.ipgeolocation.io/ipgeo?apiKey=8d6cd8bdac9a4b2f8edef66b785a45ee&fields=country_code2', { mode: 'cors' });
    const data = await response.json();
    if (data.country_code2 === 'MK') {
        const languageSelect = document.getElementById('Language');
        languageSelect.value = 'mk-MK';
    }
})();
