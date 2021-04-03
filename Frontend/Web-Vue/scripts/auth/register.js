var register = new Vue({
    el: '#register',

    data:
    {
        loading: false,

        passwordVisible: false,
        
        nameInvalid: false,
        usernameInvalid: false,
        emailInvalid: false,
        passwordInvalid: false,

        usernameMessage: '',
        passwordMessage: '',

        errorMessages: {
            usernameEmpty: 'Username required',
            passwordEmpty: 'Password required',
            usernameExists: 'Username already exists',
            usernameFormat: 'Incorrect username format',
            passwordLength: 'Password must be at least 5 characters long',
            passwordWhitespace: 'Password cannot contain whitespace'
        }
    },

    methods:
    {
        register: function () {
            let nameField = document.getElementById('registerName');
            let usernameField = document.getElementById('registerUsername');
            let emailField = document.getElementById('registerEmail');
            let passwordField = document.getElementById('registerPassword');

            let name = nameField.value;
            let username = usernameField.value;
            let email = emailField.value == '' ? null : emailField.value;
            let password = passwordField.value;

            this.usernameMessage = this.errorMessages.usernameEmpty;
            this.passwordMessage = this.errorMessages.passwordEmpty;

            this.nameInvalid = name == '';
            this.usernameInvalid = username == '';
            this.emailInvalid = false;
            this.passwordInvalid = password == '';

            if (username.includes(' ')){
                this.usernameInvalid = true;
                this.usernameMessage = this.errorMessages.usernameFormat;
            }

            if (email != null) {
                let re = /^\w+([\.-]?\w+)+@\w+([\.:]?\w+)+(\.[a-zA-Z0-9]{2,3})+$/;
                this.emailInvalid = !re.test(email);
            }

            if (password.includes(' ')) {
                this.passwordInvalid = true;
                this.passwordMessage = this.errorMessages.passwordWhitespace;
            }

            if (password.length < 5) {
                this.passwordInvalid = true;
                this.passwordMessage = this.errorMessages.passwordLength;;
            }

            if (!(this.nameInvalid || this.usernameInvalid || this.passwordInvalid)) {
                this.post(name, username, email, password);
            }
        },

        post: async function (name, username, email, password) {
            this.loading = true;

            const response = await fetch(server + 'api/authentication/register', {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: {
                    'Content-Type': 'application/json',
                    'ApiKey': apiKey
                },
                body: JSON.stringify({ Name: name, Username: username, Email: email, Password: password })
            });

            if (response.status === 409) {
                this.usernameInvalid = true;
                this.usernameMessage = this.errorMessages.usernameExists;
            }
            else if (response.status == 200) {
                const responseData = await response.json();

                appUser.set(responseData, {load: false});

                this.clear();

                let modal = bootstrap.Modal.getInstance(document.getElementById('register'));
                modal.toggle();
            }

            this.loading = false;
        },

        clear: function() {
            let nameField = document.getElementById('registerName');
            let usernameField = document.getElementById('registerUsername');
            let emailField = document.getElementById('registerEmail');
            let passwordField = document.getElementById('registerPassword');

            nameField.value = '';
            usernameField.value = '';
            emailField.value = '';
            passwordField.value = '';

            this.nameInvalid = false;
            this.usernameInvalid = false;
            this.emailInvalid = false;
            this.passwordInvalid = false;
        }
    }
});