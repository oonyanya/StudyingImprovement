function Inject_fn() {
    const MARU = 'O';
    const BATSU = 'X';
    const SANKAKU = '？';

    function add_temp_marker(elements) {
        add_temp_marker(elements, null);
    }
    function add_temp_marker(elements, matching) {
        elements.forEach((e) => {
            if (e.textContent.match(matching) == null) {
                return;
            }

            const new_item1 = document.createElement('span');
            new_item1.className= "temp_checkbox mark";
            new_item1.innerHTML = MARU;

            const new_item2 = document.createElement('span');
            new_item2.className= "temp_checkbox mark";
            new_item2.innerHTML = BATSU;

            const new_item4 = document.createElement('span');
            new_item4.className= "temp_checkbox mark";
            new_item4.innerHTML = SANKAKU;

            e.append(new_item1, new_item2, new_item4);
        });
    }

    function add_temp_marker_for_text(elements, matching) {
        elements.forEach((e) => {
            replace_element(e, (s) => {
                let new_s = s.
                    replaceAll(matching,
                        "<span class='temp_checkbox strikeline'>$1</span>"
                    );
                return new_s;
            });
        });
    }

    function isHTML(htmlString) {
        let parser = new DOMParser();
        let doc = parser.parseFromString(htmlString, "application/xml");
        let errorNode = doc.querySelector('parsererror');
        return !errorNode;
    }

    function replace_element(element, f) {
        element.innerHTML = f(element.innerHTML);
    }

    function wash_string_for_sreen_reader(s) {
        /* EDGEのスクリーンリーダーが途中で止まってしまう対策 */
        return s.replaceAll("・", "、");
    }

    function add_folding_marker(elements) {
        const id = 0;
        elements.forEach((e) => {
            replace_element(e, (s) => {
                let new_s = s.
                    replaceAll(/（/g, '<span class=\'folding_box\'>※<span>（').
                    replaceAll(/）/g, '）</span></span>');
                return wash_string_for_sreen_reader(new_s);
            });
        });
    }

    function wash_element_for_screen_reader(elements) {
        elements.forEach((e) => {
            replace_element(e, (s) => {
                return wash_string_for_sreen_reader(s);
            });
        });
    }

    function add_temp_memo(element) {
        if (element == null) return;
        let memo_element = document.createElement("textarea");
        memo_element.className = "temp_memo";
        memo_element.setAttribute("placeholder", "It is for temporary memo");
        element.append(memo_element);
    }

    let elements = document.querySelectorAll('div.question_text > div > ul > li');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > ol > li');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > table > tbody > tr > *:first-child');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > p');
    add_temp_marker(elements, /[^ァ-ヴ][ァ-ヴ]　/); //「ア　ねこねこ」に一致
    add_temp_marker(elements, /[1-9１-９]\s*((①|②|③|④|⑤|⑥|⑦|⑧|⑨)[ァ-ヴ]\s)+/); //「１ ①ア ①ア」に一致
    add_temp_marker(elements, /[1-9１-９]\s*([Ａ-Ｚ][ァ-ヴ]\s*)+/); //「１ Ａウ ２ Ａウ」に一致
    add_temp_marker_for_text(elements, /([1-9１-９]\s*[ァ-ヴ]+)/g); //「１ アウ ２ イウ」に一致
    add_temp_marker_for_text(elements, /(.+)/g);

    add_temp_memo(document.querySelector("div.question_item"));

    elements = document.querySelectorAll('#doc > h1');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h2');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h3');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > p');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > table > tbody > tr > td');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ul > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ol > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#answer_box_on > div > div > div > div > p');
    add_folding_marker(elements);

    /* video.jsを使っているようなので、モバイルデーターの節約のため自動再生を止める */
    if (typeof (videojs) != 'undefined') {
        console.log("video js hook start");
        videojs.hook('beforesetup', function (videoEl, options) {
            console.log("video js hook success");
            options.autoplay = false;
            options.preload = 'none';
            return options;
        });
    }
}

function Inject_fn_onload() {
    /* iframがある場合、どうせ後で読み込まれる */
    if (typeof (jQuery) == "undefined")
        return;
    /* 暗記マーカー機能が破壊されたので再度定義する(ソース：member.studying.jp/skin/common/js/doc.js) */
    $('.span-memory').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
    });
    /* 折り畳み */
    $('.folding_box').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
        /* 暗記ノートの部分が親要素の場合、親に影響が及ぶと変な動作をしてしまう */
        return false;
    });
    /* 一時チェック */
    $('.temp_checkbox').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
        /* 暗記ノートの部分が親要素の場合、親に影響が及ぶと変な動作をしてしまう */
        return false;
    });
}

function Inject_fn_oninteractive() {
    Inject_fn();
    window.addEventListener("load", () => {
        Inject_fn_onload();
    });
}

console.log("document state:" + document.readyState);
if (document.readyState == 'loading') {
    document.addEventListener("DOMContentLoaded", () => {
        Inject_fn_oninteractive();
    });
} else if (document.readyState == 'interactive') {
    Inject_fn_oninteractive();
} else {
    Inject_fn();
    Inject_fn_onload();
}
