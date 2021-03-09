class User {
    logged = false;
    id = null;
    name = null;
    username = null;
    about = null;
    token = null;
    favorites = [];

    constructor() {
        let storageData = JSON.parse(localStorage.getItem('user'));
        if (storageData != undefined) {
            this._setData(storageData);
            this.logged = true;
        }
    }

    update(data) {
        localStorage.setItem('user', JSON.stringify(data));
        this._setData(data);
        this.logged = true;
    }

    async getFavorites() {
        if (this.id != null) {
            const response = await fetch(`http://localhost:5000/api/users/${this.id}/favorites`);
            this.favorites = await response.json();
        }
    }

    //Use after editing user data
    updateStorage() {
        localStorage.setItem('user', JSON.stringify({
            id: this.id,
            name: this.name,
            username: this.username,
            about: this.about,
            token: this.token
        }));
    }

    logout() {
        this.logged = false;
        this.id = null;
        this.name = null;
        this.username = null;
        this.about = null;
        this.token = null;
        this.favorites = [];
        localStorage.removeItem('user');
    }

    _setData(data) {
        this.id = data.id;
        this.name = data.name;
        this.username = data.username;
        this.about = data.about;
        this.token = data.token;
    }
}

var user = new User();
user.getFavorites();