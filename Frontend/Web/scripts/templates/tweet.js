var tweet = {
  props: ['entry', 'avatar', 'showRetweets', 'user', 'deleteEntry'],
  methods:
  {
    displayCount: function (n) {
      if (n >= 1e6) {
        let res = Math.round(n / 1e5) / 10;

        return res.toString() + 'M';
      }

      else if (n >= 1e3) {
        let res = Math.round(n / 1e2) / 10;

        return res.toString() + 'K';
      }

      return n <= 1 ? null : n;
    },

    liked: function (user, id) {
      if (!user.logged) {
        return false;
      }

      return user.favorites.includes(id);
    },

    retweeted: function (user, id) {
      if (!user.logged) {
        return false;
      }

      return user.retweets.some(x => x.id == id);
    },

    addLike: function (user, entry) {
      if (!user.logged) {
        return;
      }

      user.favorites.push(entry.id);
      entry.likeCount++;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    removeLike: function (user, entry) {
      if (!user.logged) {
        return;
      }

      let index = user.favorites.indexOf(entry.id);
      user.favorites.splice(index, 1);
      entry.likeCount--;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    retweet: function (user, entry) {
      if (!user.logged || user.id == entry.authorId) {
        return;
      }

      user.retweets.push(entry);
      entry.retweetCount++;
      user.addEntry(entry);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    removeRetweet: function (user, entry) {
      if (!user.logged) {
        return;
      }

      let index = user.retweets.findIndex(x => x.id == entry.id);
      user.retweets.splice(index, 1);
      entry.retweetCount--;
      user.deleteEntry(entry.id);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    openComments: function (entry) {
      comment.load(entry);
    }
  },
  template: `
    <div class="row border-bottom p-3">
      <div class="pb-3" v-if="showRetweets && entry.authorId != user.id">
        <i class="la la-retweet" aria-hidden="true"></i>
        {{ user.name }} Retweeted <br>
      </div>

      <div class="col-auto">
        <img v-bind:src='avatar' class="img rounded-circle"
          style="width: 50px; height: 50px; outline: none;" alt="avatar">
      </div>

      <div class="col-auto">
        <b style="font-size: large;">{{ entry.author.name }}</b> @{{ entry.author.username }}

        <button type="button" class="btn btn-default" aria-label="Left Align"
          v-on:click='deleteEntry(entry.id)' v-if="entry.authorId == user.id">
          <span class="la la-trash" aria-hidden="true"></span>
        </button> <br>
        <a style="color: black; word-wrap: break-word;">{{ entry.text }}</a> <br>

        <div class="btn-group" role="group" aria-label="Basic example">
          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align"
           v-on:click="openComments(entry)" data-bs-toggle="modal" data-bs-target="#comment">
            <span class="la la-comment-dots" aria-hidden="true"></span>
            <span>{{ displayCount(entry.commentCount) }}</span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align">
            <span class="la la-retweet" aria-hidden="true" v-bind:style="retweeted(user, entry.id) ? 'color: blue;': ''"
            v-on:click="retweeted(user, entry.id) ? removeRetweet(user, entry): retweet(user, entry)"></span>
            <span>{{ displayCount(entry.retweetCount) }}</span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align">
            <span class="la la-heart" aria-hidden="true" v-bind:style="liked(user, entry.id) ? 'color: blue;': ''"
             v-on:click="liked(user, entry.id) ? removeLike(user, entry): addLike(user, entry)"></span>
            <span>{{ displayCount(entry.likeCount) }}</span>
          </button>
        </div>

      </div>
    </div>`
}