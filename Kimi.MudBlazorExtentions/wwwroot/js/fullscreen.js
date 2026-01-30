
// wwwroot/js/fullscreen.js

// ---------- Fullscreen API helpers ----------
function supportsFullscreenApi() {
    return !!(Element.prototype.requestFullscreen || Element.prototype.webkitRequestFullscreen);
}

function enterFullscreen(el) {
    const req = el.requestFullscreen || el.webkitRequestFullscreen;
    if (req) return req.call(el, { navigationUI: 'hide' }).catch(() => { });
    return Promise.reject("Fullscreen API not available");
}

function exitFullscreen() {
    const doc = document;
    const exit = doc.exitFullscreen || doc.webkitExitFullscreen;
    if (exit) return exit.call(doc).catch(() => { });
    return Promise.resolve();
}

function isFullscreenNow() {
    const doc = document;
    return !!(doc.fullscreenElement || doc.webkitFullscreenElement);
}

// ---------- Portal fallback ----------
let portal = {
    host: null,
    mounted: false,
    placeholder: null,
    originalParent: null,
    childEl: null,
};

function mountToBody(childId) {
    const el = document.getElementById(childId);
    if (!el || portal.mounted) return;

    portal.originalParent = el.parentElement;

    // 占位以便还原
    portal.placeholder = document.createElement('div');
    portal.placeholder.style.width = '0';
    portal.placeholder.style.height = '0';
    portal.originalParent.insertBefore(portal.placeholder, el);

    portal.host = document.createElement('div');
    // 使用样式类以便统一管理层级、背景、滚动等
    portal.host.className = 'toggle-fullscreen-portal-host';

    portal.childEl = el;
    portal.host.appendChild(el);
    document.body.appendChild(portal.host);
    portal.mounted = true;
}

function unmountFromBody() {
    if (!portal.mounted || !portal.host) return;

    const el = portal.childEl;
    if (el && portal.originalParent && portal.placeholder) {
        portal.originalParent.insertBefore(el, portal.placeholder);
        portal.placeholder.remove();
    }
    portal.host.remove();
    portal.host = null;
    portal.placeholder = null;
    portal.originalParent = null;
    portal.childEl = null;
    portal.mounted = false;
}

function isPortaled() {
    return !!portal.mounted;
}

// ---------- Public API ----------
/**
 * 切换全屏（系统级或 Portal）
 * @param {string} id - 目标元素 id（建议为 ChildContent 容器）
 * @param {boolean} usePortalFallback - Fullscreen API 不可用时是否启用 Portal 降级
 * @param {boolean} alwaysUsePortal - 是否总是使用 Portal，忽略 Fullscreen API（适合包含 MudSelect 等弹层的内容）
 * @returns {Promise<boolean>} - 当前是否处于广义全屏状态
 */
export async function toggleFullscreenById(id, usePortalFallback = true, alwaysUsePortal = false) {
    const el = document.getElementById(id);
    if (!el) return isFullscreenNow() || isPortaled();

    if (alwaysUsePortal) {
        if (!isPortaled()) mountToBody(id);
        else unmountFromBody();
        return isFullscreenNow() || isPortaled();
    }

    // 优先系统级全屏
    if (supportsFullscreenApi()) {
        if (!isFullscreenNow()) {
            await enterFullscreen(el);
        } else {
            await exitFullscreen();
        }
        return isFullscreenNow();
    }

    // 不支持系统级，走 Portal
    if (usePortalFallback) {
        if (!isPortaled()) {
            mountToBody(id);
        } else {
            unmountFromBody();
        }
    }

    return isFullscreenNow() || isPortaled();
}

/** 退出所有全屏（系统级 + Portal） */
export async function exitAll() {
    await exitFullscreen();
    unmountFromBody();
}

/** 广义全屏状态（系统级或 Portal） */
export function isFull() {
    return isFullscreenNow() || isPortaled();
}

/** 订阅系统级全屏事件（Portal 不触发） */
export function registerFullscreenChange(dotnetHelper, callbackMethod) {
    function handler() {
        const isFs = !!(document.fullscreenElement || document.webkitFullscreenElement);
        dotnetHelper.invokeMethodAsync(callbackMethod, isFs);
    }
    document.addEventListener('fullscreenchange', handler);
    document.addEventListener('webkitfullscreenchange', handler);

    return {
        dispose: () => {
            document.removeEventListener('fullscreenchange', handler);
            document.removeEventListener('webkitfullscreenchange', handler);
        }
    };
}