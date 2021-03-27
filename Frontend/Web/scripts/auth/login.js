var login = new Vue({
    el: '#login',
    data:
    {
        loading: false,

        passwordVisible: false,

        usernameInvalid: false,
        passwordInvalid: false,
        usernameMessage: '',
        passwordMessage: '',

        errorMessages: {
            usernameEmpty: 'Username required',
            passwordEmpty: 'Password required',
            usernameNull: 'User doesn\'t exist',
            passwordIncorrect: 'Incorrect password'
        }
    },

    methods:
    {
        login: function () {
            let usernameField = document.getElementById('loginUsername');
            let passwordField = document.getElementById('loginPassword');

            let username = usernameField.value;
            let password = passwordField.value;

            this.usernameMessage = this.errorMessages.usernameEmpty;
            this.passwordMessage = this.errorMessages.passwordEmpty;

            this.usernameInvalid = username == '';
            this.passwordInvalid = password == '';

            if (!(this.usernameInvalid || this.passwordInvalid)) {
                this.post(username, password);
            }
        },

        post: async function (username, password) {
            this.loading = true;

            const response = await fetch('http://localhost:5000/api/authentication/login', {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username: username, password: password })
            });

            const responseData = await response.json();

            if (response.status === 404) {
                this.usernameMessage = this.errorMessages.usernameNull;
                this.usernameInvalid = true;
            }
            else if (response.status === 401) {
                this.passwordMessage = this.errorMessages.passwordIncorrect;
                this.passwordInvalid = true;
            }
            else if (response.status == 200) {
                appUser.set(responseData);

                let modal = bootstrap.Modal.getInstance(document.getElementById('login'));
                modal.toggle();
            }

            this.loading = false;
        },

        clear: function() {
            let usernameField = document.getElementById('loginUsername');
            let passwordField = document.getElementById('loginPassword');

            usernameField.value = '';
            passwordField.value = '';

            this.usernameInvalid = false;
            this.passwordInvalid = false;
        }
    }
});