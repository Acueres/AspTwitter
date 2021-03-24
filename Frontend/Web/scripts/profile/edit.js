var edit = new Vue({
    el: '#edit',

    data:
    {
        appUser: appUser,
        nameInvalid: false,
        usernameInvalid: false,

        usernameMessage: '',

        errorMessages: {
            usernameEmpty: 'Username can\'t be empty',
            usernameExists: 'Username already exists',
        }
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('edit-name');
            let usernameField = document.getElementById('edit-username');
            let aboutField = document.getElementById('about');

            let name = nameField.value;
            let username = usernameField.value;
            let about = aboutField.value;

            this.nameInvalid = name == '';
            this.usernameInvalid = username == '';

            this.usernameMessage = this.errorMessages.usernameEmpty;

            const response = await fetch(`http://localhost:5000/api/users/${appUser.id}`, {
                method: 'PUT',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + appUser.token
                },
                body: JSON.stringify({
                    name: name,
                    username: username,
                    about: about
                })
            });

            if (response.status === 200) {
                appUser.name = name;
                appUser.username = username;
                appUser.about = about;

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
            else if (response.status == 409) {
                this.usernameInvalid = true;
                this.usernameMessage = this.errorMessages.usernameExists;
            }
        },

        reset: function () {
            document.getElementById('edit-name').value = appUsername;
            document.getElementById('edit-username').value = appUser.username;
            document.getElementById('about').value = appUser.about;
        }
    }
});