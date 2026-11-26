using System.IO;
using System.Net;
using System.Text;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Services
{
    public class ThreeDMapService : IThreeDMapService
    {
        public async Task<OperationResult<string>> GenerateMapAsync(ThreeDMapOptions options)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(options.OutputPath))
                {
                    return OperationResult<string>.Fail("请选择输出文件路径");
                }

                var directory = Path.GetDirectoryName(options.OutputPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var html = GenerateHtml(options);
                await File.WriteAllTextAsync(options.OutputPath, html, Encoding.UTF8);

                return OperationResult<string>.Ok(options.OutputPath, "3D 地图页面生成成功");
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Fail($"生成 3D 地图失败: {ex.Message}", ex);
            }
        }

        public string GenerateHtml(ThreeDMapOptions options)
        {
            var title = WebUtility.HtmlEncode(options.Title);
            var geoJsonUrl = EscapeJavaScript(options.GeoJsonUrl);
            var centerLongitude = NormalizeNumber(options.ProjectionCenterLongitude, "104");
            var centerLatitude = NormalizeNumber(options.ProjectionCenterLatitude, "37.5");
            var extrudeDepth = Math.Max(1, options.ExtrudeDepth);
            var topColor = NormalizeColor(options.TopColor, "#0a2a3a");
            var sideColor = NormalizeColor(options.SideColor, "#0d3b4f");
            var borderColor = NormalizeColor(options.BorderColor, "#ffffff");
            var highlightColor = NormalizeColor(options.HighlightColor, "#ffeb3b");
            var showStars = options.ShowStars ? "true" : "false";
            var showLabels = options.ShowLabels ? "true" : "false";

            return $$"""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>{{title}}</title>
  <style>
    * { box-sizing: border-box; }
    body {
      margin: 0;
      overflow: hidden;
      background: radial-gradient(circle at center, #09213a 0%, #030b16 70%);
      font-family: "Microsoft YaHei", Arial, sans-serif;
      color: #fff;
    }
    #container {
      width: 100vw;
      height: 100vh;
      position: relative;
    }
    #title {
      position: fixed;
      top: 24px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 10;
      padding: 10px 28px;
      border: 1px solid rgba(45, 239, 255, 0.5);
      border-radius: 999px;
      background: rgba(2, 16, 32, 0.58);
      box-shadow: 0 0 30px rgba(45, 239, 255, 0.25);
      letter-spacing: 4px;
      font-size: 24px;
      font-weight: 700;
      white-space: nowrap;
    }
    #tooltip {
      position: fixed;
      display: none;
      z-index: 20;
      pointer-events: none;
      padding: 8px 12px;
      border: 1px solid rgba(255, 255, 255, 0.45);
      border-radius: 6px;
      background: rgba(0, 0, 0, 0.72);
      color: #fff;
      font-size: 14px;
      box-shadow: 0 0 16px rgba(45, 239, 255, 0.35);
    }
    #panel {
      position: fixed;
      left: 24px;
      bottom: 24px;
      z-index: 10;
      max-width: 360px;
      padding: 14px 16px;
      border: 1px solid rgba(45, 239, 255, 0.28);
      border-radius: 12px;
      background: rgba(2, 16, 32, 0.55);
      line-height: 1.8;
      color: rgba(255, 255, 255, 0.84);
      font-size: 13px;
    }
    #error {
      position: fixed;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
      display: none;
      max-width: 760px;
      padding: 22px 26px;
      border: 1px solid rgba(255, 80, 80, 0.7);
      border-radius: 12px;
      background: rgba(30, 0, 0, 0.72);
      color: #ffd4d4;
      z-index: 30;
      line-height: 1.7;
    }
  </style>
</head>
<body>
  <div id="container"></div>
  <div id="title">{{title}}</div>
  <div id="tooltip"></div>
  <div id="panel">
    鼠标左键拖拽：旋转地图<br />
    鼠标滚轮：缩放视角<br />
    鼠标悬停：高亮并显示区划名称<br />
    数据源：<span id="sourceText"></span>
  </div>
  <div id="error"></div>

  <script src="https://cdn.jsdelivr.net/npm/three@0.132.2/build/three.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/three@0.132.2/examples/js/controls/OrbitControls.js"></script>
  <script src="https://d3js.org/d3-geo.v1.min.js"></script>
  <script>
    const config = {
      geoJsonUrl: '{{geoJsonUrl}}',
      center: [{{centerLongitude}}, {{centerLatitude}}],
      extrudeDepth: {{extrudeDepth}},
      topColor: '{{topColor}}',
      sideColor: '{{sideColor}}',
      borderColor: '{{borderColor}}',
      highlightColor: '{{highlightColor}}',
      showStars: {{showStars}},
      showLabels: {{showLabels}}
    };

    document.getElementById('sourceText').textContent = config.geoJsonUrl;

    const container = document.getElementById('container');
    const tooltip = document.getElementById('tooltip');
    const errorBox = document.getElementById('error');

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 10000);
    camera.position.set(0, -190, 185);

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setSize(window.innerWidth, window.innerHeight);
    container.appendChild(renderer.domElement);

    const controls = new THREE.OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.08;
    controls.rotateSpeed = 0.65;
    controls.zoomSpeed = 0.8;

    const mapGroup = new THREE.Group();
    scene.add(mapGroup);

    const raycaster = new THREE.Raycaster();
    const mouse = new THREE.Vector2();
    let hoveredMesh = null;
    const clickableMeshes = [];

    const defaultTopColor = new THREE.Color(config.topColor);
    const highlightColor = new THREE.Color(config.highlightColor);

    function showError(message) {
      errorBox.style.display = 'block';
      errorBox.innerHTML = message;
    }

    function createStars() {
      if (!config.showStars) return;
      const geometry = new THREE.BufferGeometry();
      const positions = [];
      for (let i = 0; i < 900; i++) {
        positions.push((Math.random() - 0.5) * 900);
        positions.push((Math.random() - 0.5) * 900);
        positions.push((Math.random() - 0.5) * 500);
      }
      geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
      const material = new THREE.PointsMaterial({ color: 0x6fefff, size: 1.2, transparent: true, opacity: 0.55 });
      const stars = new THREE.Points(geometry, material);
      scene.add(stars);
    }

    function normalizePolygons(feature) {
      const geometry = feature.geometry;
      if (!geometry) return [];
      if (geometry.type === 'Polygon') return [geometry.coordinates];
      if (geometry.type === 'MultiPolygon') return geometry.coordinates;
      return [];
    }

    function ringToShape(ring, projection) {
      const shape = new THREE.Shape();
      ring.forEach((coord, index) => {
        const projected = projection(coord);
        if (!projected) return;
        const [x, y] = projected;
        if (index === 0) shape.moveTo(x, -y);
        else shape.lineTo(x, -y);
      });
      return shape;
    }

    function ringToPoints(ring, projection, z) {
      return ring.map(coord => {
        const projected = projection(coord);
        if (!projected) return null;
        const [x, y] = projected;
        return new THREE.Vector3(x, -y, z);
      }).filter(Boolean);
    }

    function addLabel(feature, polygon, projection) {
      if (!config.showLabels) return;
      const name = feature.properties && (feature.properties.name || feature.properties.fullname);
      if (!name) return;
      const center = d3.geoCentroid(feature);
      const projected = projection(center);
      if (!projected) return;

      const canvas = document.createElement('canvas');
      canvas.width = 256;
      canvas.height = 64;
      const ctx = canvas.getContext('2d');
      ctx.fillStyle = 'rgba(0, 0, 0, 0.45)';
      ctx.fillRect(0, 0, canvas.width, canvas.height);
      ctx.fillStyle = '#ffffff';
      ctx.font = '28px Microsoft YaHei';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(name, canvas.width / 2, canvas.height / 2);

      const texture = new THREE.CanvasTexture(canvas);
      const material = new THREE.SpriteMaterial({ map: texture, transparent: true });
      const sprite = new THREE.Sprite(material);
      sprite.scale.set(18, 4.5, 1);
      sprite.position.set(projected[0], -projected[1], config.extrudeDepth + 3);
      mapGroup.add(sprite);
    }

    function createFeature(feature, projection) {
      const name = feature.properties && (feature.properties.name || feature.properties.fullname || '未知区划');
      const polygons = normalizePolygons(feature);
      const featureGroup = new THREE.Group();
      featureGroup.userData = { name };

      polygons.forEach(polygon => {
        if (!polygon || !polygon[0]) return;
        const outerRing = polygon[0];
        const shape = ringToShape(outerRing, projection);
        for (let i = 1; i < polygon.length; i++) {
          const hole = ringToShape(polygon[i], projection);
          shape.holes.push(hole);
        }

        const geometry = new THREE.ExtrudeGeometry(shape, {
          depth: config.extrudeDepth,
          bevelEnabled: false
        });

        const topMaterial = new THREE.MeshBasicMaterial({
          color: config.topColor,
          transparent: true,
          opacity: 0.86
        });
        const sideMaterial = new THREE.MeshBasicMaterial({
          color: config.sideColor,
          transparent: true,
          opacity: 0.78
        });
        const mesh = new THREE.Mesh(geometry, [topMaterial, sideMaterial]);
        mesh.userData = { name, topMaterial };
        featureGroup.add(mesh);
        clickableMeshes.push(mesh);

        const linePoints = ringToPoints(outerRing, projection, config.extrudeDepth + 0.35);
        if (linePoints.length > 1) {
          const lineGeometry = new THREE.BufferGeometry().setFromPoints(linePoints);
          const lineMaterial = new THREE.LineBasicMaterial({ color: config.borderColor, transparent: true, opacity: 0.82 });
          featureGroup.add(new THREE.Line(lineGeometry, lineMaterial));
        }
      });

      addLabel(feature, polygons[0], projection);
      mapGroup.add(featureGroup);
    }

    function fitMapToView() {
      const box = new THREE.Box3().setFromObject(mapGroup);
      const center = box.getCenter(new THREE.Vector3());
      const size = box.getSize(new THREE.Vector3());
      mapGroup.position.sub(center);
      const maxSize = Math.max(size.x, size.y);
      const scale = maxSize > 0 ? 140 / maxSize : 1;
      mapGroup.scale.set(scale, scale, scale);
      mapGroup.rotation.x = -Math.PI / 7;
    }

    async function loadMap() {
      try {
        createStars();
        const response = await fetch(config.geoJsonUrl);
        if (!response.ok) throw new Error('GeoJSON 请求失败：' + response.status + ' ' + response.statusText);
        const geojson = await response.json();
        if (!geojson.features || !Array.isArray(geojson.features)) throw new Error('GeoJSON 格式不正确：缺少 features 数组');

        const projection = d3.geoMercator().center(config.center).translate([0, 0]).scale(720);
        geojson.features.forEach(feature => createFeature(feature, projection));
        fitMapToView();
      } catch (error) {
        console.error(error);
        showError('地图加载失败：' + error.message + '<br />请检查网络、GeoJSON 地址或浏览器控制台。');
      }
    }

    function onMouseMove(event) {
      mouse.x = (event.clientX / window.innerWidth) * 2 - 1;
      mouse.y = -(event.clientY / window.innerHeight) * 2 + 1;
      raycaster.setFromCamera(mouse, camera);
      const intersects = raycaster.intersectObjects(clickableMeshes, false);

      if (hoveredMesh && (!intersects.length || intersects[0].object !== hoveredMesh)) {
        hoveredMesh.userData.topMaterial.color.copy(defaultTopColor);
        hoveredMesh = null;
        tooltip.style.display = 'none';
      }

      if (intersects.length) {
        hoveredMesh = intersects[0].object;
        hoveredMesh.userData.topMaterial.color.copy(highlightColor);
        tooltip.style.display = 'block';
        tooltip.textContent = hoveredMesh.userData.name;
        tooltip.style.left = event.clientX + 14 + 'px';
        tooltip.style.top = event.clientY + 14 + 'px';
      }
    }

    function onResize() {
      camera.aspect = window.innerWidth / window.innerHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(window.innerWidth, window.innerHeight);
    }

    function animate() {
      requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    }

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('resize', onResize);

    loadMap();
    animate();
  </script>
</body>
</html>
""";
        }

        private static string NormalizeNumber(string value, string fallback)
        {
            return double.TryParse(value, out var number) ? number.ToString(System.Globalization.CultureInfo.InvariantCulture) : fallback;
        }

        private static string NormalizeColor(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            var color = value.Trim();
            if (!color.StartsWith("#")) color = "#" + color;
            return color.Length == 7 ? color : fallback;
        }

        private static string EscapeJavaScript(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", "");
        }
    }
}
