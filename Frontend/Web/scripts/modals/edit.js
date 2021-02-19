var edit = new Vue({
    el: '#edit',

    data:
    {
        nameInvalid: false,
        name: JSON.parse(localStorage.getItem('currentUser')).name,
        about: JSON.parse(localStorage.getItem('currentUser')).about
    },

    methods:
    {
        save: async function () {
            let nameField = document.getElementById('editName');

            let name = nameField.value;

            this.nameInvalid = name == '';

            const currentUser = JSON.parse(localStorage.getItem('currentUser'));

            const response = await fetch(`http://localhost:5000/api/users/${currentUser.id}`, {
                method: 'PUT',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + currentUser.token
                },
                body: JSON.stringify({
                    name: name,
                    about: this.about
                })
            });

            if (response.status === 200) {
                currentUser.name = name;
                currentUser.about = this.about;
                localStorage.setItem('currentUser', JSON.stringify(currentUser));

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit'));
                modal.toggle();
            }
        }
    },

    computed:
    {
        charactersLeft: function () {
            if (this.about == null || this.about.length == 0) {
                return null;
            }

            return this.about.length + '/' + '160';
        }
    }
});