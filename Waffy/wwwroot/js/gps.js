/**
 * Waffy GPS Map — gps.js
 * يجلب البيانات من /Map/GetMapData
 * يتبع موقع المستخدم تلقائياً
 */
(function () {
    'use strict';

    // ── State ────────────────────────────────────────────────────
    let map, userMarker, userCircle;
    let userLat = null, userLng = null;
    let allPlaces = [];
    let mapMarkers = [];
    let mode = 'services';   // 'services' | 'all'
    let searchQuery = '';

    // ── Constants ─────────────────────────────────────────────────
    const DEFAULT = [31.9539, 35.9106];

    const COLORS = {
        clinic: '#00c896',
        store: '#8b5cf6',
        petfriendly: '#f59e0b',
        playground: '#f59e0b',
        park: '#f59e0b',
        danger: '#ef4444'
    };
    const EMOJIS = {
        clinic: '🏥',
        store: '🛍️',
        petfriendly: '🌳',
        playground: '🎡',
        park: '🌳',
        danger: '🚨'
    };
    const LABELS = {
        clinic: 'Veterinary Clinic',
        store: 'Pet Store',
        petfriendly: 'Pet-Friendly',
        playground: 'Playground',
        park: 'Park',
        danger: 'Danger Report'
    };

    // ── Haversine (km) ────────────────────────────────────────────
    function distKm(lat1, lon1, lat2, lon2) {
        const R = 6371;
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLon = (lon2 - lon1) * Math.PI / 180;
        const a = Math.sin(dLat / 2) ** 2
            + Math.cos(lat1 * Math.PI / 180)
            * Math.cos(lat2 * Math.PI / 180)
            * Math.sin(dLon / 2) ** 2;
        return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    }

    function fmtDist(km) {
        return km < 1 ? `${Math.round(km * 1000)} m` : `${km.toFixed(1)} km`;
    }

    // ── Map init ──────────────────────────────────────────────────
    function initMap() {
        map = L.map('mainMap', { zoomControl: false, attributionControl: false })
            .setView(DEFAULT, 14);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19
        }).addTo(map);

        L.control.zoom({ position: 'topright' }).addTo(map);
        L.control.attribution({ prefix: '© OpenStreetMap', position: 'bottomright' }).addTo(map);

        map.on('click', function (e) {
            if (!e.originalEvent.target.closest('.leaflet-marker-icon'))
                closePopup();
        });
    }

    // ── Geolocation ───────────────────────────────────────────────
    function startTracking() {
        if (!navigator.geolocation) {
            setStatus('⚠️ Geolocation not supported', 'warn');
            return;
        }
        setStatus('📡 Locating you…', 'info');

        navigator.geolocation.watchPosition(
            onPosition,
            onPositionError,
            { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 }
        );
    }

    function onPosition(pos) {
        const { latitude: lat, longitude: lng, accuracy } = pos.coords;
        const first = userLat === null;
        userLat = lat;
        userLng = lng;

        if (userCircle) {
            userCircle.setLatLng([lat, lng]).setRadius(accuracy);
        } else {
            userCircle = L.circle([lat, lng], {
                radius: accuracy, color: '#3b82f6',
                fillColor: '#3b82f6', fillOpacity: 0.08, weight: 1
            }).addTo(map);
        }

        if (userMarker) {
            userMarker.setLatLng([lat, lng]);
        } else {
            userMarker = L.marker([lat, lng], {
                icon: L.divIcon({
                    className: '',
                    html: '<div class="wd-dot"><div class="wd-ring"></div></div>',
                    iconSize: [22, 22], iconAnchor: [11, 11]
                }),
                zIndexOffset: 1000
            }).addTo(map);
        }

        if (first) map.setView([lat, lng], 15, { animate: true });
        setStatus(`📍 Accuracy: ${Math.round(accuracy)} m`, 'ok');
        renderNearbyPanel();
    }

    function onPositionError() {
        setStatus('📍 Could not get location — showing Amman', 'warn');
    }

    // ── Fetch data from server ────────────────────────────────────
    async function fetchMapData() {
        try {
            const res = await fetch('/Map/GetMapData');
            const data = await res.json();

            allPlaces = [
                ...(data.clinics || []).map(p => ({ ...p, type: 'clinic' })),
                ...(data.stores || []).map(p => ({ ...p, type: 'store' })),
                ...(data.adminMarkers || []).map(p => ({ ...p, type: p.markerType || 'petfriendly' })),
                ...(data.dangerReports || []).map(p => ({ ...p, type: 'danger' }))
            ];

            renderMarkers();
            renderNearbyPanel();
        } catch (e) {
            console.warn('Map data error:', e);
            setStatus('⚠️ Could not load map data', 'warn');
        }
    }

    // ── Filter ────────────────────────────────────────────────────
    function visible() {
        return allPlaces.filter(p => {
            const modeOk = mode === 'all' || p.type !== 'danger';
            const searchOk = !searchQuery
                || (p.name || '').toLowerCase().includes(searchQuery)
                || (p.city || '').toLowerCase().includes(searchQuery)
                || (p.area || '').toLowerCase().includes(searchQuery)
                || (p.type || '').toLowerCase().includes(searchQuery);
            return modeOk && searchOk && p.lat && p.lng;
        });
    }

    // ── Render markers ────────────────────────────────────────────
    function renderMarkers() {
        mapMarkers.forEach(m => m.remove());
        mapMarkers = [];

        visible().forEach(place => {
            const color = COLORS[place.type] || '#97ad4d';
            const emoji = EMOJIS[place.type] || '📍';

            const icon = L.divIcon({
                className: '',
                html: `<div class="wm-pin" style="--c:${color}">${emoji}</div>
                             <div class="wm-tail" style="border-top-color:${color}"></div>`,
                iconSize: [40, 48],
                iconAnchor: [20, 48]
            });

            const m = L.marker([place.lat, place.lng], { icon }).addTo(map);
            m.on('click', () => openPopup(place));
            mapMarkers.push(m);
        });
    }

    // ── Nearby panel ──────────────────────────────────────────────
    function renderNearbyPanel() {
        const list = document.getElementById('nearbyList');
        if (!list) return;

        let places = visible().map(p => ({
            ...p,
            dist: (userLat && userLng) ? distKm(userLat, userLng, p.lat, p.lng) : null
        }));

        if (userLat) places.sort((a, b) => (a.dist ?? 999) - (b.dist ?? 999));

        if (!places.length) {
            list.innerHTML = '<div class="nb-empty">No places found</div>';
            return;
        }

        list.innerHTML = places.slice(0, 20).map(p => {
            const color = COLORS[p.type] || '#97ad4d';
            const emoji = EMOJIS[p.type] || '📍';
            const label = LABELS[p.type] || p.type;
            const sev = p.severity
                ? `<span class="nb-sev sev-${p.severity.toLowerCase()}">${p.severity}</span>` : '';
            const dist = p.dist !== null
                ? `<span class="nb-dist">${fmtDist(p.dist)}</span>` : '';

            return `
              <div class="nb-item" onclick="focusPlace(${p.lat},${p.lng})">
                <div class="nb-ico" style="background:${color}18;color:${color}">${emoji}</div>
                <div class="nb-info">
                  <div class="nb-name">${p.name}</div>
                  <div class="nb-meta">
                    <span class="nb-type" style="color:${color}">${label}</span>
                    ${dist}${sev}
                  </div>
                </div>
              </div>`;
        }).join('');
    }

    // ── Popup ─────────────────────────────────────────────────────
    function openPopup(place) {
        const color = COLORS[place.type] || '#97ad4d';
        const emoji = EMOJIS[place.type] || '📍';
        const label = LABELS[place.type] || place.type;
        const dist = (userLat && userLng)
            ? distKm(userLat, userLng, place.lat, place.lng) : null;

        const rows = [];
        if (place.city || place.area)
            rows.push(`<div class="wp-row"><span>📍</span>${[place.city, place.area].filter(Boolean).join(', ')}</div>`);
        if (dist !== null)
            rows.push(`<div class="wp-row"><span>📏</span>${fmtDist(dist)} from you</div>`);
        if (place.phone)
            rows.push(`<div class="wp-row"><span>📞</span>${place.phone}</div>`);
        if (place.hasEmergency)
            rows.push(`<div class="wp-row"><span>🚑</span>Emergency 24/7</div>`);
        if (place.severity)
            rows.push(`<div class="wp-row"><span>⚠️</span>Severity: <strong>${place.severity}</strong></div>`);
        if (place.reportCode)
            rows.push(`<div class="wp-row"><span>🔖</span>${place.reportCode}</div>`);
        if (place.notes)
            rows.push(`<div class="wp-row"><span>📝</span>${place.notes}</div>`);

        document.getElementById('wpContent').innerHTML = `
          <div class="wp-hd" style="border-left:4px solid ${color}">
            <div class="wp-emoji">${emoji}</div>
            <div class="wp-info">
              <div class="wp-name">${place.name}</div>
              <div class="wp-type" style="color:${color}">${label}</div>
            </div>
            <button class="wp-x" onclick="closePopup()">✕</button>
          </div>
          ${rows.join('')}
          <a class="wp-dir"
             href="https://www.google.com/maps/dir/?api=1&destination=${place.lat},${place.lng}"
             target="_blank">🧭 Get Directions</a>`;

        document.getElementById('waffyPopup').classList.add('show');
    }

    // ── Public ────────────────────────────────────────────────────
    window.closePopup = () =>
        document.getElementById('waffyPopup').classList.remove('show');

    window.focusPlace = (lat, lng) =>
        map.setView([lat, lng], 17, { animate: true });

    window.centerOnUser = () => {
        if (userLat && userLng) map.setView([userLat, userLng], 16, { animate: true });
        else setStatus('📡 Still locating…', 'info');
    };

    window.setMode = (newMode) => {
        mode = newMode;
        document.querySelectorAll('.mode-btn').forEach(b =>
            b.classList.toggle('active', b.dataset.mode === newMode));
        renderMarkers();
        renderNearbyPanel();
    };

    window.handleMapSearch = (val) => {
        searchQuery = val.trim().toLowerCase();
        renderMarkers();
        renderNearbyPanel();
    };

    window.toggleNearby = () =>
        document.getElementById('nearbyPanel').classList.toggle('open');

    // ── Status ────────────────────────────────────────────────────
    function setStatus(msg, type) {
        const el = document.getElementById('locStatus');
        if (!el) return;
        el.textContent = msg;
        el.dataset.type = type;
        if (type === 'ok') {
            clearTimeout(el._t);
            el._t = setTimeout(() => { el.dataset.type = 'hidden'; }, 4000);
        }
    }

    // ── Navbar ────────────────────────────────────────────────────
    function initNavbar() {
        const menuBtn = document.getElementById('menuBtn');
        const navLinks = document.getElementById('navLinks');
        if (menuBtn && navLinks) {
            menuBtn.addEventListener('click', e => {
                e.stopPropagation();
                navLinks.classList.toggle('open');
            });
            document.addEventListener('click', e => {
                if (!navLinks.contains(e.target) && e.target !== menuBtn)
                    navLinks.classList.remove('open');
            });
        }
        const sd = document.getElementById('servicesDropdown');
        const st = document.getElementById('servicesToggle');
        if (sd && st) {
            st.addEventListener('click', e => { e.stopPropagation(); sd.classList.toggle('open'); });
            document.addEventListener('click', () => sd.classList.remove('open'));
        }
        const mt = document.getElementById('mobileServicesToggle');
        const mb = document.getElementById('mobileServicesBlock');
        if (mt && mb) mt.addEventListener('click', () => mb.classList.toggle('open'));
    }

    // ── Bootstrap ─────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', () => {
        initMap();
        initNavbar();
        startTracking();
        fetchMapData();
    });

})();