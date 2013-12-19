(function ($) {

	var Translation = function (el) {
		var self = this;

		self.init(el);
	};

	Translation.prototype.init = function (el) {
		var self = this;
		this.$el = $(el);
		this.$el.data('translation', this);
		this.$domId = this.$el.attr('id');
		//this.$approve = $('.approve', this.$el);
		//this.$unapprove = $('.unapprove', this.$el);
		//this.$delete = $('.delete', this.$el);
		this.$viewActions = $('.view-actions', this.$el);
		//this.$editActions = $('.edit-actions', this.$el);
		//this.$editor = $('.inline-edit', this.$el);
		this.$display = $('p.edit', this.$el);

		$('p.edit', this.$el).off('click').on('click', function () {
		    alert("Log in to make changes.");
		    return false;
		});

		//$('p.edit', this.$el).off('click').on('click', function () {
		//	self.enterEditMode();
		//	return false;
		//});

		//this.$approve.off('click').on('click', function () {
		//	self.approve(this);
		//	return false;
		//});

		//this.$unapprove.off('click').on('click', function () {
		//	self.unapprove(this);
		//	return false;
		//});

		//this.$delete.off('click').on('click', function () {
		//	self.destroy(this);
		//	return false;
		//});

		//this.$editor.off('keydown').on('keydown', function (e) {
		//	var keyCode = e.keyCode || e.which;
		//	if (keyCode == 9) {
		//		self.update(function () {
		//			//go to next translation
		//			var other = e.shiftKey ? self.$el.prev() : self.$el.next();
		//			var t = other.data('translation');
		//			t.enterEditMode();
		//		});
		//	}
		//});

		//$('.save', this.$editActions).off('click').on('click', function () {
		//	self.update();
		//});

		var text = this.$display.data('translation-text');
		var leadingText = this.$display.data('translation-leading-text');
		if (leadingText != text) {
			var display = diffString(leadingText, text);
			this.$display.html(display);
		}
	}

	//Translation.prototype.approve = function (el) {
	//	var $el = $(el),
	//		href = $el.attr('href'),
	//		self = this;

	//	$.ajax({
	//		url: href,
	//		type: 'post',
	//		success: function (html) {
	//			self.recreate(html);
	//		}
	//	});
	//}

	//Translation.prototype.unapprove = function (el) {
	//	var $el = $(el),
	//		href = $el.attr('href'),
	//		self = this;

	//	$.ajax({
	//		url: href,
	//		type: 'post',
	//		success: function (html) {
	//			self.recreate(html);
	//		}
	//	});
	//}


	//Translation.prototype.destroy = function (el) {
	//	var $el = $(el),
	//		href = $el.attr('href'),
	//		self = this;

	//	$.ajax({
	//		url: href,
	//		type: 'post',
	//		success: function (html) {
	//			self.recreate(html);
	//		}
	//	});
	//}

	//Translation.prototype.enterEditMode = function () {
	//	var self = this;
	//	var height = $('.key', self.$el).height();
	//	self.$editor.css('height', height).parent().show();
	//	self.$display.hide();
	//	self.$viewActions.hide();
	//	self.$editActions.show();
	//	self.$editor.setCaretPosition(0);
	//}

	//Translation.prototype.update = function (callback) {
	//	var self = this,
	//		value = self.$editor.val(),
	//		key = self.$display.data('translation-key-id'),
	//		href = self.$display.data('post-url');

	//	$.ajax({
	//		type: 'post',
	//		url: href,
	//		data: { value: value },
	//		success: function (html) {
	//			self.recreate(html);
	//			if (callback)
	//				callback();
	//		}
	//	});
	//}
	Translation.prototype.enterViewMode = function () {
		var self = this;

		self.$editor.parent().hide();
		self.$display.show();
		self.$viewActions.show();
		self.$editActions.hide();
	}

	Translation.prototype.recreate = function (html) {
		var self = this,
			$html = $(html);

		self.$el.replaceWith($html);
		self.init($html);
	}


	$.fn.setCaretPosition = function (pos) {
		return this.each(function (index, elem) {
			if (elem.setSelectionRange) {
				elem.focus();
				elem.setSelectionRange(pos, pos);
			} else if (elem.createTextRange) {
				var range = elem.createTextRange();
				range.collapse(true);
				range.moveEnd('character', pos);
				range.moveStart('character', pos);
				range.select();
			}
		});
	}



	var Translations = (function () {

		var init = function ($el) {
			this.$el = $el;

			$('tr.translation', $el).each(function () {
				var t = new Translation(this);
			});
		}

		return {
			init: init
		}
	})();

	Translations.init($('table.translations'));
})(jQuery);