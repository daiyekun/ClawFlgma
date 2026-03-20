#!/bin/bash

# 认证服务测试脚本
# 用于测试 AuthService 的注册、登录、刷新令牌等功能

BASE_URL="http://localhost:5001/api/auth"

echo "=========================================="
echo "  AuthService 功能测试"
echo "=========================================="
echo ""

# 颜色定义
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 测试1: 注册新用户
echo -e "${YELLOW}[测试1] 注册新用户${NC}"
REGISTER_RESPONSE=$(curl -s -X POST "$BASE_URL/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test123456",
    "displayName": "Test User"
  }')

echo "响应: $REGISTER_RESPONSE"
echo ""

# 提取 access token 和 refresh token
ACCESS_TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.accessToken')
REFRESH_TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.refreshToken')

if [ "$ACCESS_TOKEN" != "null" ] && [ "$ACCESS_TOKEN" != "" ]; then
    echo -e "${GREEN}✓ 注册成功${NC}"
else
    echo -e "${RED}✗ 注册失败${NC}"
    exit 1
fi
echo ""

# 测试2: 使用错误密码登录
echo -e "${YELLOW}[测试2] 使用错误密码登录${NC}"
LOGIN_FAIL_RESPONSE=$(curl -s -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "WrongPassword"
  }')

echo "响应: $LOGIN_FAIL_RESPONSE"
if echo "$LOGIN_FAIL_RESPONSE" | jq -e '.message' > /dev/null; then
    echo -e "${GREEN}✓ 正确拒绝了错误密码${NC}"
else
    echo -e "${RED}✗ 应该拒绝错误密码${NC}"
fi
echo ""

# 测试3: 使用正确密码登录
echo -e "${YELLOW}[测试3] 使用正确密码登录${NC}"
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test123456"
  }')

echo "响应: $LOGIN_RESPONSE"
ACCESS_TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.accessToken')
REFRESH_TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.refreshToken')

if [ "$ACCESS_TOKEN" != "null" ] && [ "$ACCESS_TOKEN" != "" ]; then
    echo -e "${GREEN}✓ 登录成功${NC}"
else
    echo -e "${RED}✗ 登录失败${NC}"
    exit 1
fi
echo ""

# 测试4: 获取当前用户信息
echo -e "${YELLOW}[测试4] 获取当前用户信息${NC}"
ME_RESPONSE=$(curl -s -X GET "$BASE_URL/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN")

echo "响应: $ME_RESPONSE"
if echo "$ME_RESPONSE" | jq -e '.email' > /dev/null; then
    echo -e "${GREEN}✓ 获取用户信息成功${NC}"
else
    echo -e "${RED}✗ 获取用户信息失败${NC}"
fi
echo ""

# 测试5: 刷新令牌
echo -e "${YELLOW}[测试5] 刷新令牌${NC}"
REFRESH_RESPONSE=$(curl -s -X POST "$BASE_URL/refresh" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\"
  }")

echo "响应: $REFRESH_RESPONSE"
NEW_ACCESS_TOKEN=$(echo $REFRESH_RESPONSE | jq -r '.accessToken')
NEW_REFRESH_TOKEN=$(echo $REFRESH_RESPONSE | jq -r '.refreshToken')

if [ "$NEW_ACCESS_TOKEN" != "null" ] && [ "$NEW_ACCESS_TOKEN" != "" ]; then
    echo -e "${GREEN}✓ 刷新令牌成功${NC}"
    # 更新 token
    ACCESS_TOKEN=$NEW_ACCESS_TOKEN
    REFRESH_TOKEN=$NEW_REFRESH_TOKEN
else
    echo -e "${RED}✗ 刷新令牌失败${NC}"
fi
echo ""

# 测试6: 验证新令牌
echo -e "${YELLOW}[测试6] 验证新令牌${NC}"
VERIFY_RESPONSE=$(curl -s -X POST "$BASE_URL/verify" \
  -H "Content-Type: application/json" \
  -d "{
    \"token\": \"$ACCESS_TOKEN\"
  }")

echo "响应: $VERIFY_RESPONSE"
if echo "$VERIFY_RESPONSE" | jq -e '.valid' > /dev/null; then
    echo -e "${GREEN}✓ 令牌验证成功${NC}"
else
    echo -e "${RED}✗ 令牌验证失败${NC}"
fi
echo ""

# 测试7: 登出
echo -e "${YELLOW}[测试7] 登出${NC}"
LOGOUT_RESPONSE=$(curl -s -X POST "$BASE_URL/logout" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\"
  }")

echo "响应: $LOGOUT_RESPONSE"
if echo "$LOGOUT_RESPONSE" | jq -e '.message' > /dev/null; then
    echo -e "${GREEN}✓ 登出成功${NC}"
else
    echo -e "${RED}✗ 登出失败${NC}"
fi
echo ""

# 测试8: 尝试使用已撤销的 refresh token
echo -e "${YELLOW}[测试8] 尝试使用已撤销的 refresh token${NC}"
REFRESH_AGAIN_RESPONSE=$(curl -s -X POST "$BASE_URL/refresh" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\"
  }")

echo "响应: $REFRESH_AGAIN_RESPONSE"
if echo "$REFRESH_AGAIN_RESPONSE" | jq -e '.message' > /dev/null; then
    echo -e "${GREEN}✓ 正确拒绝了已撤销的 refresh token${NC}"
else
    echo -e "${RED}✗ 应该拒绝已撤销的 refresh token${NC}"
fi
echo ""

echo "=========================================="
echo -e "${GREEN}  所有测试完成!${NC}"
echo "=========================================="
