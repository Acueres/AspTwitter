class User {
    logged = false;
    id = null;
    name = null;
    username = null;
    about = null;
    token = null;

    constructor() {
        let data = JSON.parse(localStorage.getItem('user'));
        if (data != undefined) {
            this._setData(data);
            this.logged = true;
        }
    }

    set(data) {
        localStorage.setItem('user', JSON.stringify(data));
        this._setData(data);
        this.logged = true;
    }

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