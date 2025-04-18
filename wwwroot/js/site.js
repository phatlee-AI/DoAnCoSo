// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Initialize carousel
    const myCarousel = new bootstrap.Carousel(document.getElementById('mainCarousel'), {
        interval: 5000,
        ride: 'carousel'
    });

    // Load products dynamically (simulated)
    loadProducts();
    
    // Load articles dynamically (simulated)
    loadArticles();
    
    // Contact form submission
    document.getElementById('supportForm')?.addEventListener('submit', function(e) {
        e.preventDefault();
        alert('Thank you for your message! We will get back to you soon.');
        this.reset();
    });
});

// Simulated product data loading
function loadProducts() {
    const productList = document.getElementById('productList');
    if (!productList) return;

    // Simulated product data (in a real app, this would come from an API)
    const products = [
        { id: 1, name: 'Eco-Friendly Product', price: 29.99, image: 'https://via.placeholder.com/300x200?text=Eco+Product', description: 'Sustainable and environmentally friendly product.' },
        { id: 2, name: 'Organic Solution', price: 49.99, image: 'https://via.placeholder.com/300x200?text=Organic+Solution', description: '100% organic ingredients for your needs.' },
        { id: 3, name: 'Green Technology', price: 99.99, image: 'https://via.placeholder.com/300x200?text=Green+Tech', description: 'Innovative technology that saves energy.' },
        { id: 4, name: 'Recycled Materials', price: 19.99, image: 'https://via.placeholder.com/300x200?text=Recycled', description: 'Made from 100% recycled materials.' }
    ];

    // Clear loading state
    productList.innerHTML = '';

    // Add products to the page
    products.forEach(product => {
        const productCol = document.createElement('div');
        productCol.className = 'col-md-3 col-sm-6 mb-4';
        productCol.innerHTML = `
            <div class="card product-card h-100">
                <img src="${product.image}" class="card-img-top product-img" alt="${product.name}">
                <div class="card-body product-body">
                    <h5 class="card-title product-title">${product.name}</h5>
                    <p class="card-text">${product.description}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="product-price">$${product.price.toFixed(2)}</span>
                        <button class="btn btn-sm btn-success">Add to Cart</button>
                    </div>
                </div>
            </div>
        `;
        productList.appendChild(productCol);
    });
}

// Simulated article data loading
function loadArticles() {
    const articleList = document.getElementById('articleList');
    if (!articleList) return;

    // Simulated article data (in a real app, this would come from an API)
    const articles = [
        { id: 1, title: 'Sustainable Living Tips', date: 'May 15, 2023', image: 'https://via.placeholder.com/300x200?text=Sustainability', excerpt: 'Learn how to live more sustainably with these simple tips.' },
        { id: 2, title: 'Green Energy Solutions', date: 'April 28, 2023', image: 'https://via.placeholder.com/300x200?text=Green+Energy', excerpt: 'Discover the latest in renewable energy technology.' },
        { id: 3, title: 'Eco-Friendly Products', date: 'March 10, 2023', image: 'https://via.placeholder.com/300x200?text=Eco+Products', excerpt: 'Our top picks for environmentally friendly products.' },
        { id: 4, title: 'Corporate Sustainability', date: 'February 22, 2023', image: 'https://via.placeholder.com/300x200?text=Corporate', excerpt: 'How businesses can adopt sustainable practices.' }
    ];

    // Clear loading state
    articleList.innerHTML = '';

    // Add articles to the page
    articles.forEach(article => {
        const articleCol = document.createElement('div');
        articleCol.className = 'col-md-3 col-sm-6 mb-4';
        articleCol.innerHTML = `
            <div class="card article-card h-100">
                <img src="${article.image}" class="card-img-top article-img" alt="${article.title}">
                <div class="card-body article-body">
                    <span class="article-date">${article.date}</span>
                    <h5 class="card-title article-title mt-2">${article.title}</h5>
                    <p class="card-text">${article.excerpt}</p>
                    <a href="#" class="btn btn-link text-success p-0">Read More →</a>
                </div>
            </div>
        `;
        articleList.appendChild(articleCol);
    });
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
        e.preventDefault();
        
        const targetId = this.getAttribute('href');
        if (targetId === '#') return;
        
        const targetElement = document.querySelector(targetId);
        if (targetElement) {
            window.scrollTo({
                top: targetElement.offsetTop - 70,
                behavior: 'smooth'
            });
        }
    });
});

// Cập nhật giỏ hàng trong modal
        function updateCart() {
            var cartItems = getCartItems(); // Giả sử bạn có một hàm lấy các sản phẩm trong giỏ hàng
            var cartList = document.getElementById("cartItems");
            var totalPrice = 0;
            cartList.innerHTML = ""; // Xóa danh sách giỏ hàng cũ

            // Thêm các sản phẩm vào danh sách giỏ hàng
            cartItems.forEach(function(item) {
                var listItem = document.createElement("li");
                listItem.classList.add("list-group-item");
                listItem.textContent = item.name + " - " + item.price + " VND";
                cartList.appendChild(listItem);

                totalPrice += item.price; // Cộng tổng tiền
            });

            // Cập nhật tổng tiền trong modal
            document.getElementById("totalPrice").textContent = totalPrice;
        }

        // Giả sử đây là hàm lấy các sản phẩm từ giỏ hàng (ví dụ sử dụng sessionStorage hoặc localStorage)
        function getCartItems() {
            return JSON.parse(localStorage.getItem("cartItems")) || []; // Đọc giỏ hàng từ localStorage
        }

        // Khi thêm sản phẩm vào giỏ hàng
        function addToCart(product) {
            var cartItems = getCartItems();
            cartItems.push(product);
            localStorage.setItem("cartItems", JSON.stringify(cartItems)); // Lưu giỏ hàng vào localStorage
            updateCart(); // Cập nhật lại giỏ hàng
        }

        document.addEventListener("DOMContentLoaded", function () {
            // Lấy dữ liệu giỏ hàng khi trang tải xong
            fetch("/Cart/GetCartSummary")
                .then(response => response.json())
                .then(data => {
                    const cartCount = document.getElementById("cart-count");
                    const cartItemsContainer = document.getElementById("cart-items");
        
                    if (data.length > 0) {
                        // Cập nhật số lượng giỏ hàng
                        cartCount.innerText = data.length;
        
                        // Hiển thị danh sách sản phẩm trong giỏ hàng
                        cartItemsContainer.innerHTML = data.map(item => `
                            <div class="d-flex align-items-center mb-2">
                                <img src="${item.ProductImage}" class="rounded me-2" width="30" height="30" style="object-fit: cover">
                                <div>
                                    <p class="mb-0">${item.ProductName}</p>
                                    <small class="text-muted">Số lượng: ${item.Quantity}</small>
                                </div>
                                <span class="fw-bold ms-3">${item.TotalPrice.toLocaleString()}<sup>đ</sup></span>
                            </div>
                        `).join('');
                    } else {
                        // Giỏ hàng trống
                        cartItemsContainer.innerHTML = '<p class="text-center text-muted">Giỏ hàng trống</p>';
                        cartCount.innerText = '0';
                    }
                });
        });

        function updateCartDropdown() {
            $.get('/Cart/GetCartItems', function(data) {
                const cartItemsContainer = $('#cart-items');
                const cartCount = $('#cart-count');
                const cartSubtotal = $('#cart-subtotal');
                
                if (data.items && data.items.length > 0) {
                    let itemsHtml = '';
                    let total = 0;
                    
                    data.items.forEach(item => {
                        total += item.price * item.quantity;
                        itemsHtml += `
                            <div class="cart-item">
                                <img src="${item.imageUrl}" class="cart-item-img" alt="${item.productName}">
                                <div class="cart-item-details">
                                    <h6 class="cart-item-title mb-1">${item.productName}</h6>
                                    <div class="d-flex justify-content-between align-items-center">
                                        <span class="cart-item-price">${item.quantity} x ${item.price.toLocaleString()}đ</span>
                                        <button class="cart-item-remove" onclick="removeFromCart(${item.productId})">
                                            <i class="fas fa-times"></i>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        `;
                    });
                    
                    cartItemsContainer.html(itemsHtml);
                    cartCount.text(data.totalItems);
                    cartSubtotal.text(total.toLocaleString() + 'đ');
                } else {
                    cartItemsContainer.html(`
                        <div class="text-center py-3">
                            <i class="fas fa-shopping-cart fa-2x text-muted mb-2"></i>
                            <p class="text-muted mb-0">Giỏ hàng trống</p>
                        </div>
                    `);
                    cartCount.text('0');
                    cartSubtotal.text('0đ');
                }
            });
        }
        
        // Function to remove item from cart
        function removeFromCart(productId) {
            $.post('/Cart/RemoveFromCart', { productId: productId }, function() {
                updateCartDropdown();
            });
        }
        
        // Initialize cart dropdown when page loads
        $(document).ready(function() {
            updateCartDropdown();
            
            // Update cart when items are added (you'll need to call this when adding items)
            $(document).on('cartUpdated', function() {
                updateCartDropdown();
            });
        });
        
