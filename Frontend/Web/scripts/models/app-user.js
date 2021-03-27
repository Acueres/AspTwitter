class AppUser extends User {
    logged = false;
    token = null;
    recommended = [];

    constructor() {
        super(null);
        this.init();
    }

    async init() {
        let storageData = JSON.parse(localStorage.getItem('auth'));

        if (storageData == undefined) {
            this._getRecommended();
            return;
        }

        const response = await fetch('http://localhost:5000/api/authentication/test', {
            method: 'GET',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + storageData.token
            }
        });

        if (response.status == 200) {
            this.id = storageData.id;
            this.token = storageData.token;

            this.logged = true;

            this.load();
        }

        this._getRecommended();
    }

    set(data) {
        localStorage.setItem('auth', JSON.stringify(data));
        this.id = data.id;
        this.token = data.token;

        this.load();
        this.logged = true;

        this._getRecommended();
    }

    addEntry(entry) {
        this.entries.splice(0, 0, entry);
    }

    deleteEntry(id) {
        let index = this.entries.findIndex(x => x.id == id);
        if (index != -1) {
            this.entries.splice(index, 1);
        }
    }

    editEntry(id, text) {
        let index = this.entries.findIndex(x => x.id == id);
        if (index != -1) {
            this.entries[index].text = text;
        }
    }

    clear() {
        this.logged = false;
        this.id = null;
        this.name = null;
        this.username = null;
        this.about = null;
        this.token = null;
        this.favorites = [];
        this.retweets = [];
        this.followers = [];
        this.following = [];
        this.followingCount = 0;
        this.followerCount = 0;
        localStorage.removeItem('auth');

        this._getRecommended();
    }

    follows(id) {
        return this.following.some(x => x.id == id);
    }

    async follow(user) {
        const response = await fetch(`http://localhost:5000/api/users/${user.id}/follow`, {
            method: 'POST',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + this.token
            }
        });

        if (response.status == 200) {
            this.following.push(user);
            this.followingCount++;
        }
    }

    async unfollow(user) {
        const response = await fetch(`http://localhost:5000/api/users/${user.id}/unfollow`, {
            method: 'DELETE',
            cache: 'no-cache',
            credentials: 'omit',
            redirect: 'follow',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + this.token
            }
        });

        if (response.status == 200) {
            let index = this.following.indexOf(user.id);
            this.following.splice(index, 1);
            this.followingCount--;
        }
    }

    async _getRecommended() {
        let id = 0;
        let count = 3;
        if (this.logged) {
            id = this.id;
        }

        const response = await fetch(`http://localhost:5000/api/users/${id}/recommended/${count}`);
        this.recommended = await response.json();
        this.recommended.reverse();
    }
}

var appUser = new AppUser();