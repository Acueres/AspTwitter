var register = new Vue({
    el: '#register',

    data:
    {
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
            passwordIncorrect: 'Password must be at least 5 characters long'
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

            if (!(this.nameInvalid || this.usernameInvalid || this.passwordInvalid)) {
                this.post(name, username, email, password);
            }
        },

        post: async function (name, username, email, password) {
            const response = await fetch('http://localhost:5000/api/users/register', {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Name: name, Username: username, Email: email, Password: password })
            });

            const responseData = await response.json();

            if (response.status === 500) {
                this.usernameInvalid = true;
                this.usernameMessage = this.errorMessages.usernameExists;
            }
            else {
                localStorage.setItem('currentUser', JSON.stringify(responseData));
                console.log(responseData);

                main.logged = true;
                main.username = username;

                let modal = bootstrap.Modal.getInstance(document.getElementById('register'));
                modal.toggle();

                location.reload();
            }
        }
    }
});